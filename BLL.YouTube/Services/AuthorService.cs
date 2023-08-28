using BLL.YouTube.Base;
using BLL.YouTube.Extensions;
using DAL.EF.Extensions;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using YoutubeDLSharp.Metadata;

namespace BLL.YouTube.Services;

public class AuthorService : BaseYouTubeService
{
    private readonly Dictionary<string, Author> _cachedAuthors = new();

    public AuthorService(IServiceProvider services) : base(services)
    {
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

    private async Task<ICollection<Author>> AddOrGetAuthors(IEnumerable<AuthorFetchArg> authorFetchArgs)
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

                Ctx.Authors.Add(author);
                Ctx.RegisterAuthorAddedCallback(new PlatformEntityAddedEventArgs(
                    author.Id, EPlatform.YouTube, author.IdOnPlatform));
                _cachedAuthors.TryAdd(arg.AuthorId, author);
                authors.Add(author);
            }
        }

        return authors;
    }
}

internal record AuthorFetchArg(string AuthorId, Func<Author> NewAuthorFunc);