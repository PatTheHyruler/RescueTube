using System.Collections.Concurrent;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Contracts;
using RescueTube.Core.Utils;
using RescueTube.Domain;
using RescueTube.Domain.Entities;
using RescueTube.YouTube.Base;
using RescueTube.YouTube.Utils;
using YoutubeDLSharp;

namespace RescueTube.YouTube.Services;

public partial class VideoDownloadService : BaseYouTubeService, IPlatformVideoDownloadService
{
    private readonly AppPaths _appPaths;

    private static ThrottlingAssessmentWithValidity? LatestThrottlingAssessment { get; set; }

    public VideoDownloadService(
        IServiceProvider services,
        ILogger<VideoDownloadService> logger,
        AppPaths appPaths
    ) : base(services, logger)
    {
        _appPaths = appPaths;
    }

    public bool IsLikelyThrottled()
    {
        return LatestThrottlingAssessment?.ShouldSkipDownloading() ?? false;
    }

    public DataFetchDefinition DataFetchDefinition => YouTubeConstants.DataFetches.YtDlp.VideoFileDownload;

    public async Task<string> DownloadVideoAsync(Video video, CancellationToken ct = default)
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

        if (!result.Success)
        {
            throw new ApplicationException($"YouTube video download failed: {result.ErrorOutputToString()}");
        }

        var videoFilePath = result.Data.AssertNotNull();

        try
        {
            var infoJsonPath = PathUtils.GetFilePathWithoutExtension(videoFilePath) + ".info.json";
            video.InfoJsonPath = _appPaths.GetPathRelativeToDownloads(infoJsonPath);
            video.InfoJson = await File.ReadAllTextAsync(infoJsonPath, ct);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to set video info JSON for video {VideoId}", video.Id);
        }

        return videoFilePath;
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