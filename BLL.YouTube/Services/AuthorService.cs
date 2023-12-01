using BLL.Events;
using BLL.Events.Events;
using BLL.YouTube.Base;
using BLL.YouTube.Extensions;
using DAL.EF.Extensions;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YoutubeDLSharp.Metadata;

namespace BLL.YouTube.Services;

public class AuthorService : BaseYouTubeService
{
    private readonly Dictionary<string, Author> _cachedAuthors = new();
    private readonly IMediator _mediator;

    public AuthorService(IServiceProvider services, ILogger<AuthorService> logger, IMediator mediator) : base(services, logger)
    {
        _mediator = mediator;
    }

    private async Task<Author> AddOrGetAuthor(VideoData videoData)
    {
        return await AddOrGetAuthor(videoData.ChannelID, videoData.ToDomainAuthor);
    }

    public async Task AddAndSetAuthor(Video video, VideoData videoData)
    {
        var author = await AddOrGetAuthor(videoData);
        Ctx.VideoAuthors.SetVideoAuthor(video.Id, author.Id);
    }

    private async Task<Author> AddOrGetAuthor(string id, Func<Author> newAuthorFunc)
    {
        return (await AddOrGetAuthors(new[] { new AuthorFetchArg(id, newAuthorFunc) })).First();
    }

    internal async Task<ICollection<Author>> AddOrGetAuthors(IEnumerable<AuthorFetchArg> authorFetchArgs)
    {
        ICollection<Author> authors = new List<Author>();
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
            await Ctx.Authors.Filter(EPlatform.YouTube, notCachedIds.Select(e => e.AuthorId)).ToListAsync();

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
                try
                {
                    // TODO: Move this to a background job
                    var channel = await YouTubeUow.YouTubeExplodeClient.Channels.GetAsync(author.IdOnPlatform);
                    author.AuthorImages = channel.Thumbnails.Select(e => new AuthorImage
                    {
                        ImageType = EImageType.ProfilePicture,
                        LastFetched = DateTime.UtcNow,

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
                    author.FailedExtraDataFetchAttempts++;
                    Logger.LogError(e, "Error occurred fetching extra author data for {Platform} author {IdOnPlatform}",
                        EPlatform.YouTube, arg.AuthorId);
                }

                Ctx.Authors.Add(author);
                Ctx.RegisterSavedChangesCallbackRunOnce(() =>
                    _mediator.Publish(new AuthorAddedEvent(
                        author.Id, EPlatform.YouTube, author.IdOnPlatform)));
                _cachedAuthors.TryAdd(arg.AuthorId, author);
                authors.Add(author);
            }
        }

        return authors;
    }
}

internal record AuthorFetchArg(string AuthorId, Func<Author> NewAuthorFunc);