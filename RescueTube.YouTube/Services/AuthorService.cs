using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Data.Extensions;
using RescueTube.Core.Events;
using RescueTube.Core.Mediator;
using RescueTube.Core.Services;
using RescueTube.Core.Utils;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;
using RescueTube.YouTube.Base;
using RescueTube.YouTube.Utils;
using YoutubeDLSharp.Metadata;

namespace RescueTube.YouTube.Services;

public class AuthorService : BaseYouTubeService
{
    private readonly Dictionary<string, Author> _cachedAuthors = new();
    private readonly IMediator _mediator;

    /// <summary>
    /// Last YouTubeExplode exception time (probably means we hit rate limit)
    /// </summary>
    public DateTimeOffset LastYtExplodeRateLimitHit { get; private set; } = DateTimeOffset.MinValue;

    public AuthorService(IServiceProvider services, ILogger<AuthorService> logger, IMediator mediator) : base(services,
        logger)
    {
        _mediator = mediator;
    }

    public async Task TryFetchAuthorVideosAsync(Guid authorId, bool force, CancellationToken ct = default)
    {
        const string fetchType = YouTubeConstants.FetchTypes.YtDlp.ChannelVideos;

        var author = await DbCtx.Authors
            .Where(a => a.Id == authorId)
            .Include(a => a.ArchivalSettings)
            .Include(a => a.DataFetches!.Where(df =>
                df.Source == YouTubeConstants.FetchTypes.YtDlp.Source
                && df.Type == fetchType))
            .Include(a => a.AuthorImages!)
            .ThenInclude(ai => ai.Image)
            .FirstAsync(ct);

        if (!force && !ShouldFetchAuthorVideos(author))
        {
            return;
        }

        var authorResult = await YouTubeUow.YoutubeDl.RunVideoDataFetch(Url.ToAuthorUrl(author.IdOnPlatform), ct: ct);

        if (authorResult is not { Success: true, Data.Entries: not null, Data.Entries.Length: > 0 })
        {
            Logger.LogInformation("Failed to fetch videos for author {AuthorId}", authorId);
            await _mediator.Send(new AddFailedDataFetchRequest
            {
                Type = fetchType,
                Source = YouTubeConstants.FetchTypes.YtDlp.Source,
                ShouldAffectValidity = false,
                AuthorId = authorId,
            }, ct);
            return;
        }

        var domainAuthorData = authorResult.Data.ToDomainAuthorFromChannel(fetchType);
        ServiceUow.EntityUpdateService.UpdateAuthor(author, domainAuthorData, false,
            new EntityUpdateService.UpdateAuthorOptions
            {
                ImageUpdateOptions = EntityUpdateService.EImageUpdateOptions.OnlyAdd,
            });

        await YouTubeUow.VideoService.AddOrUpdateVideosFromAuthorVideosFetchAsync(
            authorResult.Data, author, fetchType, ct);
    }

    private bool ShouldFetchAuthorVideos(Author author)
    {
        switch (author)
        {
            case { ArchivalSettingsId: null }:
                Logger.LogWarning("Author {AuthorId} has no ArchivalSettings", author.Id);
                return false;
            case { ArchivalSettings: null }:
                Logger.LogWarning("ArchivalSettings not loaded for author {AuthorId}", author.Id);
                return false;
            case { ArchivalSettings.Active: false }:
                Logger.LogWarning("Author {AuthorId} not enabled for archival", author.Id);
                return false;
        }

        var latestAllowedVideosFetchTime = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(1));
        var latestDataFetch = author.DataFetches
            .AssertNotNull($"{nameof(author.DataFetches)} not loaded for author {author.Id}")
            .MaxBy(d => d.OccurredAt);
        if (latestDataFetch != null && latestDataFetch.OccurredAt > latestAllowedVideosFetchTime)
        {
            Logger.LogInformation("Skipping videos fetch for author {AuthorId}, latest data fetch: {LatestDataFetch}",
                author.Id,
                new { latestDataFetch.OccurredAt, latestDataFetch.Id, latestDataFetch.Source, latestDataFetch.Type });
            return false;
        }

        return true;
    }

    public async Task<Author> AddOrGetAuthor(YoutubeExplode.Channels.Channel channel, CancellationToken ct = default)
    {
        return await AddOrGetAuthor(channel.Id, channel.ToDomainAuthor, ct);
    }

    public async Task<Author> AddOrGetAuthor(VideoData videoData, string fetchType, CancellationToken ct = default)
    {
        return await AddOrGetAuthor(videoData.ChannelID, () => videoData.ToDomainAuthorFromVideo(fetchType), ct);
    }

    public async Task AddAndSetAuthor(Video video, VideoData videoData, string fetchType,
        CancellationToken ct = default)
    {
        var author = await AddOrGetAuthor(videoData, fetchType, ct);
        await AddAndSetAuthor(video, author, ct);
    }

    public async Task AddAndSetAuthor(Video video, Author author, CancellationToken ct = default)
    {
        bool hasAuthor;
        if (video.VideoAuthors == null)
        {
            hasAuthor = await DbCtx.VideoAuthors
                .Where(va => va.Author!.Platform == EPlatform.YouTube
                             && va.Author!.IdOnPlatform == author.IdOnPlatform
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
            DbCtx.VideoAuthors.SetVideoAuthor(video.Id, author.Id);
        }
    }

    private async Task<Author> AddOrGetAuthor(string id, Func<Author> newAuthorFunc, CancellationToken ct = default)
    {
        return (await AddOrGetAuthors(new[] { new AuthorFetchArg(id, newAuthorFunc) }, ct)).First();
    }

    internal async Task<ICollection<Author>> AddOrGetAuthors(IEnumerable<AuthorFetchArg> authorFetchArgs,
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
            await DbCtx.Authors.Filter(EPlatform.YouTube, notCachedIds.Select(e => e.AuthorIdOnPlatform))
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

                DbCtx.Authors.Add(author);
                DataUow.RegisterSavedChangesCallbackRunOnce(() =>
                    _mediator.Publish(new AuthorAddedEvent(
                        author.Id, EPlatform.YouTube, author.IdOnPlatform), ct));
                _cachedAuthors.TryAdd(arg.AuthorIdOnPlatform, author);
                authors.Add(author);
            }
        }

        return authors;
    }

    public async Task TryUpdateWithYouTubeExplodeDataAsync(Guid authorId, CancellationToken ct = default)
    {
        var author = await DbCtx.Authors
            .Where(a => a.Id == authorId)
            .Include(a => a.AuthorImages!)
            .ThenInclude(ai => ai.Image!)
            .FirstAsync(cancellationToken: ct);
        var newAuthorData = await TryFetchExtraYouTubeExplodeAuthorDataAsync(author.IdOnPlatform, ct);
        ServiceUow.EntityUpdateService.UpdateAuthor(author, newAuthorData, false, new()
        {
            ImageUpdateOptions = EntityUpdateService.EImageUpdateOptions.OnlyAdd,
        });
    }

    private async Task<Author> TryFetchExtraYouTubeExplodeAuthorDataAsync(string idOnPlatform, CancellationToken ct)
    {
        try
        {
            return await FetchExtraYouTubeExplodeAuthorDataAsync(idOnPlatform, ct);
        }
        catch (Exception e)
        {
            LastYtExplodeRateLimitHit = DateTimeOffset.UtcNow;
            Logger.LogError(e, "YouTubeExplode data fetch failed for {Platform} author {AuthorIdOnPlatform}",
                EPlatform.YouTube, idOnPlatform);
            return new Author
            {
                IdOnPlatform = idOnPlatform,
                DataFetches =
                [
                    new DataFetch
                    {
                        OccurredAt = DateTimeOffset.UtcNow,
                        ShouldAffectValidity = true,
                        Source = YouTubeConstants.FetchTypes.YouTubeExplode.Source,
                        Type = YouTubeConstants.FetchTypes.YouTubeExplode.Channel,
                        Success = false,
                    },
                ],
            };
        }
    }

    private async Task<Author> FetchExtraYouTubeExplodeAuthorDataAsync(string idOnPlatform,
        CancellationToken ct = default)
    {
        var channel = await YouTubeUow.YouTubeExplodeClient.Channels.GetAsync(idOnPlatform, ct);

        return new Author
        {
            IdOnPlatform = idOnPlatform,
            DisplayName = channel.Title,
            AuthorImages = channel.Thumbnails.Select(e => new AuthorImage
            {
                ImageType = EImageType.ProfilePicture,
                LastFetched = DateTimeOffset.UtcNow,

                Image = new Image
                {
                    Platform = EPlatform.YouTube,

                    Width = e.Resolution.Width,
                    Height = e.Resolution.Height,
                    Url = e.Url,
                },
            }).ToList(),
            DataFetches =
            [
                new DataFetch
                {
                    OccurredAt = DateTimeOffset.UtcNow,
                    ShouldAffectValidity = true,
                    Source = YouTubeConstants.FetchTypes.YouTubeExplode.Source,
                    Type = YouTubeConstants.FetchTypes.YouTubeExplode.Channel,
                    Success = true,
                }
            ],
        };
    }
}

internal record AuthorFetchArg(string AuthorIdOnPlatform, Func<Author> NewAuthorFunc);