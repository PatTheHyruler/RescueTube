using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Data;
using RescueTube.Core.Data.Extensions;
using RescueTube.Core.DataFetches;
using RescueTube.Core.Events;
using RescueTube.Core.Services;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;
using RescueTube.YouTube.Base;
using RescueTube.YouTube.Utils;
using YoutubeDLSharp.Metadata;
using YoutubeExplode.Channels;

namespace RescueTube.YouTube.Services;

public class AuthorService : BaseYouTubeService
{
    private readonly AppDbContext _dbCtx;
    private readonly ILogger<AuthorService> _logger;
    private readonly EntityUpdateService _entityUpdateService;
    private readonly IMediator _mediator;
    private readonly YouTubeServices _youTubeServices;
    private readonly DataFetchService _dataFetchService;

    private readonly Dictionary<string, Author> _cachedAuthors = new();

    public AuthorService(AppDbContext dbCtx, ILogger<AuthorService> logger, EntityUpdateService entityUpdateService, IMediator mediator, YouTubeServices youTubeServices, DataFetchService dataFetchService)
    {
        _dbCtx = dbCtx;
        _logger = logger;
        _entityUpdateService = entityUpdateService;
        _mediator = mediator;
        _youTubeServices = youTubeServices;
        _dataFetchService = dataFetchService;
    }

    /// <summary>
    /// Last YouTubeExplode exception time (probably means we hit rate limit)
    /// </summary>
    public static DateTimeOffset LastYtExplodeRateLimitHit { get; private set; } = DateTimeOffset.MinValue;

    public async Task TryFetchAuthorVideosAsync(Guid authorId, CancellationToken ct = default)
    {
        using var logScope = _logger.BeginScope(nameof(TryFetchAuthorVideosAsync) + " AuthorId: {AuthorId}", authorId);
        var dataFetchDefinition = YouTubeConstants.DataFetches.YtDlp.ChannelVideos;

        var author = await _dbCtx.Authors
            .Where(a => a.Id == authorId)
            .Include(a => a.ArchivalSettings)
            .Include(a => a.AuthorImages!)
            .ThenInclude(ai => ai.Image)
            .FirstAsync(ct);

        await using var dataFetchScope = await _dataFetchService.StartDataFetchAsync(
            dataFetchDefinition, author, ct);
        if (dataFetchScope is null)
        {
            _logger.LogWarning("Channel videos data fetch for author {AuthorId} is already ongoing, skipping duplicate fetch", authorId);
            return;
        }

        var dataFetch = dataFetchScope.DataFetch;

        _logger.LogInformation("Fetching videos for author {AuthorId}", authorId);

        var authorResult = await _youTubeServices.YoutubeDl.RunVideoDataFetch(Url.ToAuthorUrl(author.IdOnPlatform), ct: ct);

        _logger.LogInformation("Fetched videos for author {AuthorId}", authorId);

        if (authorResult is not { Success: true, Data.Entries: not null, Data.Entries.Length: > 0 })
        {
            _logger.LogError("Failed to fetch videos for author {AuthorId}", authorId);
            await _dataFetchService.UpdateDataFetchStatusAsync(dataFetch, DataFetchStatus.Failed,
                message: authorResult?.ErrorOutputToString());
            return;
        }

        dataFetch.Status = DataFetchStatus.Succeeded;
        dataFetch.DataFetchResults.Add(new DataFetchResult { Author = author, AuthorId = authorId });

        var domainAuthorData = authorResult.Data.ToDomainAuthorFromChannel();
        _entityUpdateService.UpdateAuthor(author, domainAuthorData, false,
            new EntityUpdateService.UpdateAuthorOptions
            {
                ImageUpdateOptions = EntityUpdateService.EImageUpdateOptions.OnlyAdd,
            });

        await _youTubeServices.VideoService.AddOrUpdateVideosFromAuthorVideosFetchAsync(
            authorResult.Data, author, dataFetch, ct);

        await _dbCtx.SaveChangesAsync(ct);
    }

    public async Task<Author> AddOrGetAuthor(Channel channel, DataFetch dataFetch, CancellationToken ct = default)
    {
        return await AddOrGetAuthor(channel.Id, channel.ToDomainAuthor, dataFetch, ct);
    }

    public async Task<Author> AddOrGetAuthor(VideoData videoData, DataFetch dataFetch, CancellationToken ct = default)
    {
        return await AddOrGetAuthor(videoData.ChannelID, videoData.ToDomainAuthorFromVideo, dataFetch, ct);
    }

    public async Task AddAndSetAuthor(Video video, VideoData videoData, DataFetch dataFetch,
        CancellationToken ct = default)
    {
        var author = await AddOrGetAuthor(videoData, dataFetch, ct);
        await AddAndSetAuthor(video, author, ct);
    }

    public async Task AddAndSetAuthor(Video video, Author author, CancellationToken ct = default)
    {
        bool hasAuthor;
        if (video.VideoAuthors == null)
        {
            hasAuthor = await _dbCtx.VideoAuthors
                .Where(va => va.Author!.Platform == EPlatform.YouTube
                             && va.Author!.IdOnPlatform == author.IdOnPlatform
                             && va.VideoId == video.Id
                             && va.Role == EAuthorRole.Publisher)
                .AnyAsync(cancellationToken: ct);
        }
        else
        {
            hasAuthor = video.VideoAuthors.Any(va =>
                va.Author != null
                && va.Author.Platform == EPlatform.YouTube
                && va.Author.IdOnPlatform == author.IdOnPlatform
                && va.Role == EAuthorRole.Publisher);
        }

        if (!hasAuthor)
        {
            _dbCtx.VideoAuthors.SetVideoAuthor(video.Id, author.Id);
        }
    }

    private async Task<Author> AddOrGetAuthor(string id, Func<Author> newAuthorFunc, DataFetch dataFetch, CancellationToken ct = default)
    {
        return (await AddOrGetAuthors([new AuthorFetchArg(id, newAuthorFunc)], dataFetch, ct)).First();
    }

    internal async Task<ICollection<Author>> AddOrGetAuthors(IEnumerable<AuthorFetchArg> authorFetchArgs,
        DataFetch dataFetch,
        CancellationToken ct = default)
    {
        var authors = new List<Author>();
        var notCachedIds = new List<AuthorFetchArg>();
        foreach (var arg in authorFetchArgs)
        {
            var author = _cachedAuthors.GetValueOrDefault(arg.AuthorIdOnPlatform);
            if (author != null)
            {
                authors.Add(author);
            }
            else
            {
                notCachedIds.Add(arg);
            }
        }

        var fetchedAuthors =
            await _dbCtx.Authors.Filter(EPlatform.YouTube, notCachedIds.Select(e => e.AuthorIdOnPlatform))
                .ToListAsync(cancellationToken: ct);

        foreach (var arg in notCachedIds)
        {
            var fetchedAuthor = fetchedAuthors.FirstOrDefault(a => a.IdOnPlatform == arg.AuthorIdOnPlatform);
            if (fetchedAuthor != null)
            {
                _cachedAuthors.TryAdd(arg.AuthorIdOnPlatform, fetchedAuthor);
                authors.Add(fetchedAuthor);
            }
            else
            {
                var author = arg.NewAuthorFunc();

                _dbCtx.Authors.Add(author);

                dataFetch.DataFetchResults.Add(new DataFetchResult { Author = author, AuthorId = author.Id });

                await _mediator.Publish(new AuthorAddedEvent(
                        author.Id, EPlatform.YouTube, author.IdOnPlatform), ct);
                _cachedAuthors.TryAdd(arg.AuthorIdOnPlatform, author);
                authors.Add(author);
            }
        }

        return authors;
    }

    public async Task<Channel?> FetchYouTubeExplodeChannelAsync(
        string idOnPlatform, string? idType, CancellationToken ct = default)
    {
        var channel = idType switch
        {
            YouTubeConstants.IdTypes.Author.Handle => await _youTubeServices.YouTubeExplodeClient.Channels
                .GetByHandleAsync(Url.AuthorHandleRemovePrefix(idOnPlatform), ct),
            _ => await _youTubeServices.YouTubeExplodeClient.Channels.GetAsync(idOnPlatform, ct),
        };

        return channel;
    }

    public async Task TryUpdateWithYouTubeExplodeDataAsync(Guid authorId, CancellationToken ct = default)
    {
        var author = await _dbCtx.Authors
            .Where(a => a.Id == authorId)
            .Include(a => a.AuthorImages!)
            .ThenInclude(ai => ai.Image!)
            .FirstAsync(cancellationToken: ct);
        var dataFetchDefinition = YouTubeConstants.DataFetches.YouTubeExplode.Channel;
        await using var dataFetchScope = await _dataFetchService.StartDataFetchAsync(
            dataFetchDefinition, author, ct);
        dataFetchScope.ThrowIfAlreadyFetching();

        var dataFetch = dataFetchScope.DataFetch;

        try
        {
            var newAuthorData = await FetchExtraYouTubeExplodeAuthorDataAsync(author.IdOnPlatform, ct);

            dataFetch.Status = DataFetchStatus.Succeeded;
            dataFetch.DataFetchResults.Add(new DataFetchResult { Author = author, AuthorId = author.Id });

            _entityUpdateService.UpdateAuthor(author, newAuthorData, isNew: false, new()
            {
                ImageUpdateOptions = EntityUpdateService.EImageUpdateOptions.OnlyAdd,
            });

            await _dbCtx.SaveChangesAsync(ct);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to fetch videos for author {AuthorId}", authorId);
            await _dataFetchService.UpdateDataFetchStatusAsync(dataFetch, DataFetchStatus.Failed, message: e.ToString());
        }
    }

    private async Task<Author> FetchExtraYouTubeExplodeAuthorDataAsync(string idOnPlatform,
        CancellationToken ct = default)
    {
        var channel = await _youTubeServices.YouTubeExplodeClient.Channels.GetAsync(idOnPlatform, ct);

        return new Author
        {
            IdOnPlatform = idOnPlatform,
            DisplayName = channel.Title,
            AuthorImages = channel.Thumbnails.Select(e => new AuthorImage
                {
                    LastFetched = DateTimeOffset.UtcNow,

                    Image = new Image
                    {
                        Platform = EPlatform.YouTube,

                        Width = e.Resolution.Width,
                        Height = e.Resolution.Height,
                        Url = e.Url,
                    },
                })
                .Select(ImageUtils.TrySetImageType)
                .ToList(),
        };
    }
}

internal record AuthorFetchArg(string AuthorIdOnPlatform, Func<Author> NewAuthorFunc);