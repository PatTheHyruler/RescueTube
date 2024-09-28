using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Data.Extensions;
using RescueTube.Core.Events;
using RescueTube.Core.Mediator;
using RescueTube.Core.Services;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;
using RescueTube.YouTube.Base;
using RescueTube.YouTube.Utils;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;

namespace RescueTube.YouTube.Services;

public class VideoService : BaseYouTubeService
{
    private readonly IMediator _mediator;

    public VideoService(IServiceProvider services, ILogger<VideoService> logger, IMediator mediator)
        : base(services, logger)
    {
        _mediator = mediator;
    }

    public async Task<VideoData?> FetchVideoDataYtdlAsync(string id, bool fetchComments, CancellationToken ct = default)
    {
        var videoResult = await YouTubeUow.YoutubeDl.RunVideoDataFetch(
            Url.ToVideoUrl(id), fetchComments: fetchComments, ct: ct);
        if (videoResult is not { Success: true })
        {
            ct.ThrowIfCancellationRequested();

            // TODO: Add status change if video exists in archive

            return null;
        }

        return videoResult.Data;
    }

    public async Task<Video?> AddOrUpdateVideoAsync(Guid videoId, CancellationToken ct = default)
    {
        var idOnPlatform = await DbCtx.Videos
            .Where(v => v.Id == videoId && v.Platform == EPlatform.YouTube)
            .Select(v => v.IdOnPlatform).FirstAsync(ct);
        return await AddOrUpdateVideoAsync(idOnPlatform, ct);
    }

    public async Task<Video?> AddOrUpdateVideoAsync(string idOnPlatform, CancellationToken ct = default)
    {
        var videoData = await FetchVideoDataYtdlAsync(idOnPlatform, false, ct);
        return videoData == null
            ? null
            : await AddOrUpdateVideoAsync(videoData, YouTubeConstants.FetchTypes.YtDlp.VideoPage, ct);
    }

    public Task<Video> AddOrUpdateVideoAsync(VideoData videoData, string fetchType, CancellationToken ct = default) =>
        AddOrUpdateVideoAsync(videoData: videoData, fetchType: fetchType, author: null, ct: ct);

    public async Task<Video> AddOrUpdateVideoAsync(VideoData videoData, string fetchType, Author? author,
        CancellationToken ct = default)
    {
        var video = await DbCtx.Videos
            .Where(v => v.Platform == EPlatform.YouTube && v.IdOnPlatform == videoData.ID)
            .Include(v => v.Title)
            .ThenInclude(t => t!.Translations)
            .Include(v => v.Description)
            .ThenInclude(t => t!.Translations)
            .Include(v => v.VideoTags)
            .Include(v => v.VideoStatisticSnapshots)
            .Include(v => v.Captions)
            .Include(v => v.VideoImages!)
            .ThenInclude(vi => vi.Image)
            .Include(v => v.VideoFiles)
            //Authors etc???
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken: ct);
        var isNew = video == null;
        video ??= new Video { IdOnPlatform = videoData.ID };
        var newVideoData = videoData.ToDomainVideo(fetchType);
        ServiceUow.EntityUpdateService.UpdateVideo(video, newVideoData, isNew,
            EntityUpdateService.EImageUpdateOptions.OnlyAdd);

        if (author == null)
        {
            await TryAddAuthorAsync(video, videoData, fetchType, ct);
        }
        else
        {
            if (isNew)
            {
                DbCtx.VideoAuthors.SetVideoAuthor(video.Id, author.Id);
            }
            else
            {
                await YouTubeUow.AuthorService.AddAndSetAuthor(video, author, ct);
            }
        }

        if (isNew)
        {
            DbCtx.Videos.Add(video);
            await _mediator.Publish(new VideoAddedEvent(video.Id, EPlatform.YouTube, video.IdOnPlatform), ct);
        }

        return video;
    }

    public async Task AddOrUpdateVideosFromAuthorVideosFetchAsync(VideoData authorData, Author author,
        string fetchType, CancellationToken ct)
    {
        foreach (var fakePlaylistOrVideoData in authorData.Entries)
        {
            await HandleVideoDataFromAuthorVideosFetchAsync(fakePlaylistOrVideoData, author, fetchType, ct);
        }
    }

    private async Task HandleVideoDataFromAuthorVideosFetchAsync(VideoData fakePlaylistOrVideoData, Author author,
        string fetchType,
        CancellationToken ct = default, EVideoType? parentVideoType = null, uint depth = 0)
    {
        if (depth > 1)
        {
            Logger.LogCritical("Unexpectedly large recursion depth, skipping. Title: {VideoDataTitle}",
                fakePlaylistOrVideoData.Title);
            return;
        }

        if (fakePlaylistOrVideoData.Entries is not null)
        {
            EVideoType? videoType = null;
            if (fakePlaylistOrVideoData.Title?.EndsWith(" - Shorts") ?? false)
            {
                videoType = EVideoType.Short;
            }

            foreach (var data in fakePlaylistOrVideoData.Entries)
            {
                await HandleVideoDataFromAuthorVideosFetchAsync(data, author, fetchType, ct, videoType, depth + 1);
            }
        }
        else
        {
            var video = await AddOrUpdateVideoAsync(fakePlaylistOrVideoData, fetchType, author, ct);
            if (parentVideoType != null)
            {
                video.Type ??= parentVideoType;
            }
        }
    }

    private async Task TryAddAuthorAsync(Video video, VideoData videoData, string fetchType,
        CancellationToken ct = default)
    {
        try
        {
            await YouTubeUow.AuthorService.AddAndSetAuthor(video, videoData, fetchType, ct);
            DbCtx.DataFetches.Add(new DataFetch
            {
                VideoId = video.Id,
                Video = video,
                OccurredAt = DateTimeOffset.UtcNow,
                Source = YouTubeConstants.FetchTypes.General.Source,
                Type = YouTubeConstants.FetchTypes.General.VideoAuthor,
                ShouldAffectValidity = false,
                Success = true,
            });
        }
        catch (Exception e)
        {
            DbCtx.DataFetches.Add(new DataFetch
            {
                VideoId = video.Id,
                Video = video,
                OccurredAt = DateTimeOffset.UtcNow,
                Source = YouTubeConstants.FetchTypes.General.Source,
                Type = YouTubeConstants.FetchTypes.General.VideoAuthor,
                ShouldAffectValidity = false,
                Success = false,
            });
            Logger.LogError(e, "Failed to add author for YouTube video {VideoId}, Author ID {AuthorId} ({AuthorName})",
                videoData.ID, videoData.ChannelID, videoData.Channel);
        }
    }
}