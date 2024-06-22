using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Events.Events;
using RescueTube.Core.Utils;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Entities.Localization;
using RescueTube.Domain.Enums;
using RescueTube.YouTube.Base;
using RescueTube.YouTube.Extensions;
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

    public async Task<Video?> AddVideoAsync(string id, CancellationToken ct = default)
    {
        var videoData = await FetchVideoDataYtdlAsync(id, false, ct);
        return videoData == null ? null : await AddVideoAsync(videoData, ct);
    }

    private Video CreateVideoFromVideoData(VideoData videoData)
    {
        var video = new Video
        {
            Platform = EPlatform.YouTube,
            IdOnPlatform = videoData.ID,

            Title = new TextTranslationKey
            {
                Translations = new List<TextTranslation>
                {
                    new()
                    {
                        Content = videoData.Title,
                    }
                }
            },
            Description = new TextTranslationKey
            {
                Translations = new List<TextTranslation>
                {
                    new()
                    {
                        Content = videoData.Description,
                    }
                }
            },

            Duration = videoData.Duration != null ? TimeSpan.FromSeconds(videoData.Duration.Value) : null,

            VideoStatisticSnapshots = new List<VideoStatisticSnapshot>
            {
                new()
                {
                    ViewCount = videoData.ViewCount,
                    LikeCount = videoData.LikeCount,
                    DislikeCount = videoData.DislikeCount,
                    CommentCount = videoData.CommentCount,

                    ValidAt = DateTimeOffset.UtcNow,
                }
            },

            Captions = videoData.Subtitles?
                .SelectMany(kvp => kvp.Value.Select(subtitleData => new Caption
                {
                    Culture = kvp.Key,
                    Ext = subtitleData.Ext,
                    LastFetched = DateTimeOffset.UtcNow,
                    Name = subtitleData.Name,
                    Platform = EPlatform.YouTube,
                    Url = subtitleData.Url,
                })).ToList(),
            VideoImages = videoData.Thumbnails?.Select(e => e.ToVideoImage()).ToList(),
            VideoTags = videoData.Tags?.Select(e => new VideoTag
            {
                Tag = e,
                NormalizedTag = e
                    .ToUpper()
                    .Where(c => !char.IsPunctuation(c))
                    .Aggregate("", (current, c) => current + c)
                    .Normalize(NormalizationForm.FormKD),
            }).ToList(),

            IsLiveStreamRecording = videoData.WasLive ?? videoData.IsLive,
            LiveStreamStartedAt = (videoData.WasLive ?? videoData.IsLive ?? false) ? videoData.ReleaseTimestamp : null,

            CreatedAt = videoData.UploadDate,
            UpdatedAt = videoData.ModifiedTimestamp,
            PublishedAt = videoData.ReleaseTimestamp,

            PrivacyStatusOnPlatform = videoData.Availability.ToPrivacyStatus(),
            IsAvailable = videoData.Availability.ToPrivacyStatus().IsAvailable(),
            PrivacyStatus = EPrivacyStatus.Private,

            LastFetchUnofficial = DateTimeOffset.UtcNow,
            LastSuccessfulFetchUnofficial = DateTimeOffset.UtcNow,
            AddedToArchiveAt = DateTimeOffset.UtcNow,
        };
        return video;
    }

    public async Task<Video> AddOrUpdateVideoAsync(VideoData videoData, CancellationToken ct = default)
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
        var newVideoData = CreateVideoFromVideoData(videoData);
        await ServiceUow.EntityUpdateService.UpdateVideo(video, newVideoData, isNew);

        await TryAddAuthorAsync(video, videoData, ct);

        if (isNew)
        {
            AddVideo(video, ct);
        }

        return video;
    }

    private async Task TryAddAuthorAsync(Video video, VideoData videoData, CancellationToken ct = default)
    {
        try
        {
            await YouTubeUow.AuthorService.AddAndSetAuthor(video, videoData, ct);
        }
        catch (Exception e)
        {
            video.FailedAuthorFetches++;
            Logger.LogError(e, "Failed to add author for YouTube video {VideoId}, Author ID {AuthorId} ({AuthorName})",
                videoData.ID, videoData.ChannelID, videoData.Channel);
        }
    }

    private void AddVideo(Video video, CancellationToken ct)
    {
        DbCtx.Videos.Add(video);

        DataUow.RegisterSavedChangesCallbackRunOnce(() =>
            _mediator.Publish(new VideoAddedEvent(
                video.Id, EPlatform.YouTube, video.IdOnPlatform), ct));
    }

    public async Task<Video> AddVideoAsync(VideoData videoData, CancellationToken ct = default)
    {
        var video = CreateVideoFromVideoData(videoData);
        await TryAddAuthorAsync(video, videoData, ct);
        AddVideo(video, ct);
        
        // TODO: Comments callback subscribe
        // TODO: Captions downloader callback subscribe

        // TODO: Categories, Games

        return video;
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
            await DbCtx.Videos
                .Where(e => e.Id == video.Id)
                .ExecuteUpdateAsync(e => e.SetProperty(
                        v => v.FailedDownloadAttempts,
                        v => v.FailedDownloadAttempts + 1),
                    cancellationToken: ct);
            throw new ApplicationException(errorString ?? $"Failed to download video {video.Id}");
        }

        video.FailedDownloadAttempts = 0;
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