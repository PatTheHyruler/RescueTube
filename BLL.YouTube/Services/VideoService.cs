using System.Text;
using BLL.Utils;
using BLL.YouTube.Base;
using BLL.YouTube.Extensions;
using BLL.YouTube.Utils;
using Domain.Entities;
using Domain.Entities.Localization;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YoutubeDLSharp.Metadata;

namespace BLL.YouTube.Services;

public class VideoService : BaseYouTubeService
{
    public VideoService(IServiceProvider services, ILogger<VideoService> logger) : base(services, logger)
    {
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

        Ctx.RegisterVideoAddedCallback(new PlatformEntityAddedEventArgs(
            video.Id, EPlatform.YouTube, videoData.ID));
        // TODO: Comments callback subscribe
        // TODO: Captions downloader callback subscribe

        // TODO: Categories, Games

        return video;
    }

    private static readonly ConcurrentHashSet<Guid> CurrentlyDownloadingVideoIds = new();

    public async Task DownloadVideos(IEnumerable<Guid>? videoIds, CancellationToken ct)
    {
        if (ct.IsCancellationRequested) return;
        var query = Ctx.Videos
            .Where(e => e.Platform == EPlatform.YouTube)
            .Include(e => e.VideoFiles)
            .Where(e => e.VideoFiles!.Count == 0 && e.FailedDownloadAttempts == 0);
        if (videoIds != null)
        {
            query = query.Where(e => videoIds.Contains(e.Id));
        }

        var videos = await query.ToListAsync(cancellationToken: ct);

        foreach (var video in videos)
        {
            if (ct.IsCancellationRequested) break;
            try
            {
                await DownloadVideo(video, ct);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error occurred downloading {Platform} video {IdOnPlatform}",
                    video.Platform, video.IdOnPlatform);
            }
        }
    }

    private async Task DownloadVideo(Video video, CancellationToken ct = default)
    {
        if (CurrentlyDownloadingVideoIds.Contains(video.Id))
        {
            Logger.LogInformation("Ignoring download request for video {VideoId} because it's already being downloaded",
                video.Id);
            return;
        }

        CurrentlyDownloadingVideoIds.Add(video.Id);

        Logger.LogInformation("Started downloading video {IdOnPlatform} on platform {Platform}",
            video.IdOnPlatform, video.Platform);
        var result = await YouTubeUow.YoutubeDl.RunVideoDownload(Url.ToVideoUrl(video.IdOnPlatform), ct: ct,
            overrideOptions: YouTubeUow.DownloadOptions);
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
        else
        {
            Logger.LogError("Failed to download {Platform} video with ID {IdOnPlatform}.\nErrors: [{Errors}]",
                EPlatform.YouTube, video.IdOnPlatform,
                result.ErrorOutput.Length > 0 ? string.Join("\n", result.ErrorOutput) : "Unknown errors");
            video.FailedDownloadAttempts++;
        }

        CurrentlyDownloadingVideoIds.Remove(video.Id);
    }
}