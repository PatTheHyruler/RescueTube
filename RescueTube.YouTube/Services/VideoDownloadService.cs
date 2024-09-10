using System.Collections.Concurrent;
using System.Globalization;
using System.Text.RegularExpressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Mediator;
using RescueTube.Core.Utils;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;
using RescueTube.YouTube.Base;
using RescueTube.YouTube.Utils;
using YoutubeDLSharp;

namespace RescueTube.YouTube.Services;

public partial class VideoDownloadService : BaseYouTubeService
{
    private readonly AppPaths _appPaths;
    private readonly IMediator _mediator;

    public static ThrottlingAssessmentWithValidity? LatestThrottlingAssessment { get; private set; }

    public VideoDownloadService(
        IServiceProvider services,
        ILogger<VideoDownloadService> logger,
        AppPaths appPaths,
        IMediator mediator
    ) : base(services, logger)
    {
        _appPaths = appPaths;
        _mediator = mediator;
    }

    public async Task<(RunResult<string> Result, Video Video)> DownloadVideoAsync(Guid videoId, CancellationToken ct)
    {
        var query = DbCtx.Videos
            .Where(e => e.Platform == EPlatform.YouTube)
            .Include(e => e.VideoFiles)
            .Where(e => e.VideoFiles!.Count == 0)
            .Where(e => e.Id == videoId);

        var video = await query.FirstAsync(ct);
        return (await DownloadVideoAsync(video, ct), video);
    }

    private async Task<RunResult<string>> DownloadVideoAsync(Video video, CancellationToken ct = default)
    {
        Logger.LogInformation("Started downloading video {IdOnPlatform} on platform {Platform}",
            video.IdOnPlatform, video.Platform);

        var downloadSpeedMonitor = new DownloadSpeedMonitor(Logger);
        var downloadProgressHandler = new AggregateProgressHandler<DownloadProgress>(
            new DownloadProgressLogger(Logger),
            downloadSpeedMonitor
        );
        // TODO: Add way to see download progress on the video page itself

        var result = await YouTubeUow.YoutubeDl.RunVideoDownload(Url.ToVideoUrl(video.IdOnPlatform), ct: ct,
            overrideOptions: YouTubeUow.DownloadOptions, progress: downloadProgressHandler);
        var throttlingAssessment = downloadSpeedMonitor.GetThrottlingAssessment();
        Logger.LogInformation("Video download finished, average download speed: {DownloadSpeed} B/s",
            downloadSpeedMonitor.AverageDownloadSpeed);
        LatestThrottlingAssessment = new ThrottlingAssessmentWithValidity(throttlingAssessment, DateTimeOffset.UtcNow);
        return result;
    }

    public async Task PersistVideoDownloadResultAsync(RunResult<string> result, Video video,
        CancellationToken ct = default)
    {
        if (!result.Success)
        {
            var errorString = result.ErrorOutput.Length > 0 ? string.Join("\n", result.ErrorOutput) : null;
            Logger.LogError("Failed to download {Platform} video with ID {IdOnPlatform}.\nErrors: [{Errors}]",
                EPlatform.YouTube, video.IdOnPlatform,
                errorString);
            await _mediator.Send(new AddFailedDataFetchRequest
            {
                Type = YouTubeConstants.FetchTypes.YtDlp.VideoFileDownload,
                Source = YouTubeConstants.FetchTypes.YtDlp.Source,
                ShouldAffectValidity = false,
                VideoId = video.Id,
                Message = errorString,
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

    private class DownloadProgressLogger : IProgress<DownloadProgress>
    {
        private readonly ILogger _logger;

        private float _previousProgress = -1;
        private DateTimeOffset _previousProcessedUpdateOccurredAt = DateTimeOffset.MinValue;
        private DownloadState? _previousDownloadState;

        public DownloadProgressLogger(ILogger logger)
        {
            _logger = logger;
        }

        private bool ShouldLog(DownloadProgress progress)
        {
            if (progress.State != _previousDownloadState)
            {
                return true;
            }

            if (Math.Abs(progress.Progress - _previousProgress) > 0.0005)
            {
                return true;
            }

            return DateTimeOffset.UtcNow - _previousProcessedUpdateOccurredAt > TimeSpan.FromMinutes(2);
        }

        public void Report(DownloadProgress value)
        {
            if (!ShouldLog(value))
            {
                return;
            }

            _previousProcessedUpdateOccurredAt = DateTimeOffset.UtcNow;
            _previousProgress = value.Progress;
            _previousDownloadState = value.State;
            _logger.LogInformation(
                "Yt-dlp download progress: {ProgressPercentage}%, ETA: {ETA}, Speed: {DownloadSpeed}, TotalDownloadSize: {TotalDownloadSize}, State: {State}",
                value.Progress * 100, value.ETA, value.DownloadSpeed, value.TotalDownloadSize, value.State);
        }
    }

    private partial class DownloadSpeedMonitor : IProgress<DownloadProgress>
    {
        private readonly ConcurrentBag<double?> _downloadSpeeds = [];
        private readonly ILogger _logger;

        public DownloadSpeedMonitor(ILogger logger)
        {
            _logger = logger;
        }

        private const double CutoffThrottlingSpeedBytes = 400 * 1024;

        public double? AverageDownloadSpeed => _downloadSpeeds.Where(v => v.HasValue).Average();

        public ThrottlingAssessment GetThrottlingAssessment()
        {
            var valueCount = _downloadSpeeds.Count(v => v.HasValue);
            var valueProportion = (float)valueCount / _downloadSpeeds.Count;
            var average = _downloadSpeeds.Where(v => v.HasValue).Average();
            if (!average.HasValue)
            {
                return new ThrottlingAssessment(false, 0);
            }

            return new ThrottlingAssessment(average.Value < CutoffThrottlingSpeedBytes, valueProportion);
        }

        public void Report(DownloadProgress value)
        {
            if (value.State != DownloadState.Downloading)
            {
                return;
            }

            if (value.DownloadSpeed is null)
            {
                _downloadSpeeds.Add(null);
                return;
            }

            var match = MyRegex().Match(value.DownloadSpeed);
            if (!match.Success)
            {
                _downloadSpeeds.Add(null);
                return;
            }

            var downloadSpeedGroup = match.Groups["value"];
            if (!double.TryParse(downloadSpeedGroup.Value, CultureInfo.InvariantCulture, out var downloadSpeed))
            {
                _downloadSpeeds.Add(null);
                return;
            }

            var infoUnitGroup = match.Groups["infoUnit"];
            try
            {
                var downloadSpeedInBytes = GetValueInBytes(downloadSpeed, infoUnitGroup.Value);
                _downloadSpeeds.Add(downloadSpeedInBytes);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to parse download speed");
                _downloadSpeeds.Add(null);
            }
        }

        private static double GetValueInBytes(double downloadSpeed, string infoUnit)
        {
            return infoUnit switch
            {
                "B" => downloadSpeed,
                "KiB" => downloadSpeed * 1024,
                "MiB" => downloadSpeed * 1024 * 1024,
                "GiB" => downloadSpeed * 1024 * 1024 * 1024,
                "TiB" => downloadSpeed * 1024 * 1024 * 1024 * 1024,
                _ => throw new ArgumentException($"Invalid download speed unit '{infoUnit}'", nameof(infoUnit)),
            };
        }

        [GeneratedRegex(@"(?<value>\d+(\.\d+))(?<speedUnit>(?<infoUnit>(?<infoPrefix>Ki|Mi|Gi|Ti)?B)/s)",
            RegexOptions.ExplicitCapture)]
        private static partial Regex MyRegex();
    }

    public readonly struct ThrottlingAssessment(bool isLikelyThrottled, float confidence)
    {
        public bool IsLikelyThrottled { get; } = isLikelyThrottled;
        public float Confidence { get; } = confidence;
    }

    public class ThrottlingAssessmentWithValidity(ThrottlingAssessment assessment, DateTimeOffset validAt)
    {
        private ThrottlingAssessment Assessment { get; } = assessment;
        private DateTimeOffset ValidAt { get; } = validAt;

        public bool ShouldSkipDownloading()
        {
            if (DateTimeOffset.UtcNow - ValidAt > TimeSpan.FromMinutes(10))
            {
                return false;
            }

            return Assessment.IsLikelyThrottled && Assessment.Confidence > 0.5;
        }
    }
}