using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Data.Extensions;
using RescueTube.Core.Events.Events;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;
using RescueTube.YouTube.Base;
using RescueTube.YouTube.Extensions;
using YoutubeDLSharp.Metadata;

namespace RescueTube.YouTube.Services;

public class AuthorService : BaseYouTubeService
{
    private readonly Dictionary<string, Author> _cachedAuthors = new();
    private readonly IMediator _mediator;
    private DateTimeOffset _lastYtExplodeRateLimitHit = DateTimeOffset.MinValue;

    public AuthorService(IServiceProvider services, ILogger<AuthorService> logger, IMediator mediator) : base(services, logger)
    {
        _mediator = mediator;
    }

    private async Task<Author> AddOrGetAuthor(VideoData videoData, CancellationToken ct = default)
    {
        return await AddOrGetAuthor(videoData.ChannelID, videoData.ToDomainAuthor, ct);
    }

    public async Task AddAndSetAuthor(Video video, VideoData videoData, CancellationToken ct = default)
    {
        var author = await AddOrGetAuthor(videoData, ct);
        DbCtx.VideoAuthors.SetVideoAuthor(video.Id, author.Id);
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
            var author = _cachedAuthors.GetValueOrDefault(arg.AuthorId);
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
            await DbCtx.Authors.Filter(EPlatform.YouTube, notCachedIds.Select(e => e.AuthorId))
                .ToListAsync(cancellationToken: ct);

        foreach (var arg in notCachedIds)
        {
            var fetchedAuthor = fetchedAuthors.FirstOrDefault(a => a.IdOnPlatform == arg.AuthorId);
            if (fetchedAuthor != null)
            {
                _cachedAuthors.TryAdd(arg.AuthorId, fetchedAuthor);
                authors.Add(fetchedAuthor);
            }
            else
            {
                var author = arg.NewAuthorFunc();
                await TryFetchExtraAuthorData(author, arg, ct); // TODO: Move this to a background job

                DbCtx.Authors.Add(author);
                DataUow.RegisterSavedChangesCallbackRunOnce(() =>
                    _mediator.Publish(new AuthorAddedEvent(
                        author.Id, EPlatform.YouTube, author.IdOnPlatform), ct));
                _cachedAuthors.TryAdd(arg.AuthorId, author);
                authors.Add(author);
            }
        }

        return authors;
    }

    private async Task TryFetchExtraAuthorData(Author author, AuthorFetchArg arg, CancellationToken ct = default)
    {
        if (_lastYtExplodeRateLimitHit < DateTimeOffset.Now.Subtract(TimeSpan.FromHours(1)))
        {
            try
            {
                var channel = await YouTubeUow.YouTubeExplodeClient.Channels.GetAsync(author.IdOnPlatform, ct);
                author.AuthorImages = channel.Thumbnails.Select(e => new AuthorImage
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
                }).ToList();
            }
            catch (Exception e)
            {
                _lastYtExplodeRateLimitHit = DateTimeOffset.Now;
                author.FailedExtraDataFetchAttempts++;
                Logger.LogError(e, "Error occurred fetching extra author data for {Platform} author {IdOnPlatform}",
                    EPlatform.YouTube, arg.AuthorId);
            }
        }
        else
        {
            Logger.LogInformation("Skipped fetching extra data for {Platform} author {IdOnPlatform}, latest rate limit hit was at {LatestRateLimitHit}",
                EPlatform.YouTube, arg.AuthorId, _lastYtExplodeRateLimitHit);
        }
    }
}

internal record AuthorFetchArg(string AuthorId, Func<Author> NewAuthorFunc);