using System.Text;
using BLL.Events;
using BLL.Events.Events;
using BLL.YouTube.Base;
using BLL.YouTube.Extensions;
using BLL.YouTube.Utils;
using Domain.Entities;
using Domain.Entities.Localization;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YoutubeDLSharp.Metadata;

namespace BLL.YouTube.Services;

public class VideoService : BaseYouTubeService
{
    private readonly IMediator _mediator;

    public VideoService(IServiceProvider services, ILogger<VideoService> logger, IMediator mediator) : base(services,
        logger)
    {
        _mediator = mediator;
    }

    public async Task<VideoData?> FetchVideoDataYtdl(string id, bool fetchComments, CancellationToken ct = default)
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

    public async Task<Video?> AddVideo(string id, CancellationToken ct = default)
    {
        var videoData = await FetchVideoDataYtdl(id, false, ct);
        return videoData == null ? null : await AddVideo(videoData);
    }

    public async Task<Video> AddVideo(VideoData videoData)
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

                    ValidAt = DateTime.UtcNow,
                }
            },

            Captions = videoData.Subtitles
                .SelectMany(kvp => kvp.Value.Select(subtitleData => new Caption
                {
                    Culture = kvp.Key,
                    Ext = subtitleData.Ext,
                    LastFetched = DateTime.UtcNow,
                    Name = subtitleData.Name,
                    Platform = EPlatform.YouTube,
                    Url = subtitleData.Url,
                })).ToList(),
            VideoImages = videoData.Thumbnails.Select(e => e.ToVideoImage()).ToList(),
            VideoTags = videoData.Tags.Select(e => new VideoTag
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

            LastFetchUnofficial = DateTime.UtcNow,
            LastSuccessfulFetchUnofficial = DateTime.UtcNow,
            AddedToArchiveAt = DateTime.UtcNow,
        };

        try
        {
            await YouTubeUow.AuthorService.AddAndSetAuthor(video, videoData);
        }
        catch (Exception e)
        {
            video.FailedAuthorFetches++;
            Logger.LogError(e, "Failed to add author for YouTube video {VideoId}, Author ID {AuthorId} ({AuthorName})",
                videoData.ID, videoData.ChannelID, videoData.Channel);
        }

        Ctx.Videos.Add(video);

        Ctx.RegisterSavedChangesCallbackRunOnce(() =>
            _mediator.Publish(new VideoAddedEvent(
                video.Id, EPlatform.YouTube, videoData.ID)));
        // TODO: Comments callback subscribe
        // TODO: Captions downloader callback subscribe

        // TODO: Categories, Games

        return video;
    }

    public async Task DownloadVideo(Guid videoId, CancellationToken ct)
    {
        var query = Ctx.Videos
            .Where(e => e.Platform == EPlatform.YouTube)
            .Include(e => e.VideoFiles)
            .Where(e => e.VideoFiles!.Count == 0 && e.FailedDownloadAttempts == 0)
            .Where(e => e.Id == videoId);

        var video = await query.FirstAsync(ct);
        await DownloadVideo(video, ct);
    }

    private async Task DownloadVideo(Video video, CancellationToken ct = default)
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
            await Ctx.Videos
                .Where(e => e.Id == video.Id)
                .ExecuteUpdateAsync(e => e.SetProperty(
                        v => v.FailedDownloadAttempts,
                        v => v.FailedDownloadAttempts + 1),
                    cancellationToken: ct);
            throw new ApplicationException(errorString ?? $"Failed to download video {video.Id}");
        }

        if (result.Success)
        {
            video.FailedDownloadAttempts = 0;
            if (video.VideoFiles != null)
            {
                foreach (var videoFile in video.VideoFiles)
                {
                    if (videoFile.ValidUntil == null || videoFile.ValidUntil > DateTime.UtcNow)
                    {
                        videoFile.ValidUntil = DateTime.UtcNow;
                    }
                }
            }

            var videoFilePath = result.Data;
            var appPathOptions = Services.GetService<IOptions<AppPathOptions>>()?.Value;
            var infoJsonPath = AppPaths.GetFilePathWithoutExtension(videoFilePath) + ".info.json";
            video.InfoJsonPath = AppPaths.GetPathRelativeToDownloads(infoJsonPath, appPathOptions);
            Ctx.VideoFiles.Add(new VideoFile
            {
                FilePath = AppPaths.GetPathRelativeToDownloads(videoFilePath, appPathOptions),
                ValidSince = DateTime.UtcNow, // Questionable semantics?
                LastFetched = DateTime.UtcNow,
                Video = video,
            });
        }
    }
}