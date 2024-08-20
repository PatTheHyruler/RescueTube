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

public class VideoService : BaseYouTubeService
{
    private readonly IMediator _mediator;
    private readonly AppPaths _appPaths;

    public VideoService(IServiceProvider services, ILogger<VideoService> logger, IMediator mediator, AppPaths appPaths)
        : base(services, logger)
    {
        _mediator = mediator;
        _appPaths = appPaths;
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
            .FirstOrDefaultAsync(cancellationToken: ct);
        var isNew = video == null;
        video ??= new Video { IdOnPlatform = videoData.ID };
        var newVideoData = videoData.ToDomainVideo(fetchType);
        ServiceUow.EntityUpdateService.UpdateVideo(video, newVideoData, isNew, EntityUpdateService.EImageUpdateOptions.OnlyAdd);

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

            DataUow.RegisterSavedChangesCallbackRunOnce(() =>
                _mediator.Publish(new VideoAddedEvent(
                    video.Id, EPlatform.YouTube, video.IdOnPlatform), ct));
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
            Logger.LogCritical("Unexpectedly large recursion depth, skipping. Title: {VideoDataTitle}", fakePlaylistOrVideoData.Title);
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

    public async Task DownloadVideoAsync(Guid videoId, CancellationToken ct)
    {
        var query = DbCtx.Videos
            .Where(e => e.Platform == EPlatform.YouTube)
            .Include(e => e.VideoFiles)
            .Where(e => e.VideoFiles!.Count == 0)
            .Where(e => e.Id == videoId);

        var video = await query.FirstAsync(ct);
        await DownloadVideoAsync(video, ct);
    }

    private async Task DownloadVideoAsync(Video video, CancellationToken ct = default)
    {
        Logger.LogInformation("Started downloading video {IdOnPlatform} on platform {Platform}",
            video.IdOnPlatform, video.Platform);
        var result = await YouTubeUow.YoutubeDl.RunVideoDownload(Url.ToVideoUrl(video.IdOnPlatform), ct: ct,
            overrideOptions: YouTubeUow.DownloadOptions);
        // TODO: Progress display?
        if (!result.Success)
        {
            var errorString = result.ErrorOutput.Length > 0 ? string.Join("\n", result.ErrorOutput) : null;
            Logger.LogError("Failed to download {Platform} video with ID {IdOnPlatform}.\nErrors: [{Errors}]",
                EPlatform.YouTube, video.IdOnPlatform,
                errorString);
            // Since we throw an exception here, we can't expect the callers of this function to call SaveChanges()
            // Also, executing this update in the DB should avoid potential concurrency issues
            await _mediator.Send(new AddFailedDataFetchRequest
            {
                Type = YouTubeConstants.FetchTypes.YtDlp.VideoFileDownload,
                Source = YouTubeConstants.FetchTypes.YtDlp.Source,
                ShouldAffectValidity = false,
                VideoId = video.Id,
            }, ct);
            throw new ApplicationException(errorString ?? $"Failed to download video {video.Id}");
        }

        DbCtx.DataFetches.Add(new DataFetch
        {
            Video = video,
            VideoId = video.Id,
            OccurredAt = DateTimeOffset.UtcNow,
            Success = true,
            Type = YouTubeConstants.FetchTypes.YtDlp.VideoFileDownload,
            Source = YouTubeConstants.FetchTypes.YtDlp.Source,
            ShouldAffectValidity = false,
        });
        if (video.VideoFiles != null)
        {
            foreach (var videoFile in video.VideoFiles)
            {
                if (videoFile.ValidUntil == null || videoFile.ValidUntil > DateTimeOffset.UtcNow)
                {
                    videoFile.ValidUntil = DateTimeOffset.UtcNow;
                }
            }
        }

        var videoFilePath = result.Data;
        var infoJsonPath = PathUtils.GetFilePathWithoutExtension(videoFilePath) + ".info.json";
        video.InfoJsonPath = _appPaths.GetPathRelativeToDownloads(infoJsonPath);
        video.InfoJson = await File.ReadAllTextAsync(infoJsonPath, CancellationToken.None);
        DbCtx.VideoFiles.Add(new VideoFile
        {
            FilePath = _appPaths.GetPathRelativeToDownloads(videoFilePath),
            ValidSince = DateTimeOffset.UtcNow, // Questionable semantics?
            LastFetched = DateTimeOffset.UtcNow,
            Video = video,
        });
    }
}