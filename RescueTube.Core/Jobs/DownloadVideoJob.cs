using System.Collections.Concurrent;
using System.Collections.Immutable;
using Hangfire;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RescueTube.Core.Constants.DataFetches;
using RescueTube.Core.Contracts;
using RescueTube.Core.Data;
using RescueTube.Core.DataFetches;
using RescueTube.Core.Jobs.Filters;
using RescueTube.Core.Services;
using RescueTube.Core.Utils;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.Jobs;

public class DownloadVideoJob
{
    private readonly ILogger<DownloadVideoJob> _logger;
    private readonly StorageLimitService _storageLimitService;
    private readonly IDataUow _dataUow;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeProvider _timeProvider;
    private readonly AppPaths _appPaths;
    private readonly ServiceRegistry _serviceRegistry;
    private readonly DataFetchService _dataFetchService;

    public DownloadVideoJob(ILogger<DownloadVideoJob> logger, StorageLimitService storageLimitService, IDataUow dataUow, IServiceProvider serviceProvider, TimeProvider timeProvider, AppPaths appPaths, IOptions<ServiceRegistry> serviceRegistry, DataFetchService dataFetchService)
    {
        _logger = logger;
        _storageLimitService = storageLimitService;
        _dataUow = dataUow;
        _serviceProvider = serviceProvider;
        _timeProvider = timeProvider;
        _appPaths = appPaths;
        _dataFetchService = dataFetchService;
        _serviceRegistry = serviceRegistry.Value;
    }

    private static readonly ConcurrentDictionary<Guid, DateTimeOffset> DownloadingVideoIds = new();

    [AutomaticRetry(Attempts = 0)]
    [DisableConcurrentExecution("download-video:{0}", timeoutSec: 5)]
    [Queue(JobQueues.Critical)]
    public async Task DownloadVideoAsync(Guid videoId, CancellationToken ct)
    {
        if (!DownloadingVideoIds.TryAdd(videoId, _timeProvider.GetUtcNow()))
        {
            _logger.LogError("Video {VideoId} is already downloading", videoId);
            return;
        }

        var video = await _dataUow.Ctx.Videos
            .Where(v => v.Id == videoId)
            .Include(v => v.VideoFiles)
            .FirstAsync(ct);

        await DownloadVideoAsync(video, ct);
    }

    [AutomaticRetry(Attempts = 0)]
    [SkipConcurrent("core:download-not-downloaded-video-recurring")]
    [Queue(JobQueues.HighPriority)]
    public async Task DownloadNextNotDownloadedVideoAsync(CancellationToken ct)
    {
        if (await _storageLimitService.IsVideoDownloadForbiddenAsync(ct))
        {
            _logger.LogWarning("Skipping video download, storage limit reached");
            return;
        }

        var downloadingVideoIds = DownloadingVideoIds.Keys.ToImmutableHashSet();
        var supportedPlatforms = _serviceRegistry.GetSupportedPlatforms<IPlatformVideoDownloadService>().AsEnumerable();
        var video = await _dataUow.Ctx.Videos
            .Where(v => supportedPlatforms.Contains(v.Platform))
            .Where(v => v.VideoFiles!.Count == 0)
            .Where(v => _dataUow.Ctx.DataFetches
                .Where(d => _dataUow.DataFetches.IsVideoDataFetch.Invoke(d, v))
                .Where(d => d.Type == DataFetchTypes.VideoFileDownload)
                .OrderByDescending(d => d.OccurredAt)
                .Take(3)
                .Count(d => d.Status != DataFetchStatus.Succeeded) < 3)
            .Where(v => !downloadingVideoIds.Contains(v.Id))
            .Include(v => v.VideoFiles)
            .OrderByDescending(v => v.ArchivalSettings.DownloadPriority)
            .ThenBy(v => v.Id)
            .FirstOrDefaultAsync(ct);

        if (video is null)
        {
            return;
        }

        // TODO: Use DataFetchContext here instead?
        if (!DownloadingVideoIds.TryAdd(video.Id, _timeProvider.GetUtcNow()))
        {
            _logger.LogError("Video {VideoId} is already downloading", video.Id);
            return;
        }

        await DownloadVideoAsync(video, ct);
    }

    private async Task DownloadVideoAsync(Video video, CancellationToken ct)
    {
        try
        {
            var platformVideoDownloadService = _serviceProvider.GetRequiredKeyedService<IPlatformVideoDownloadService>(video.Platform);
            if (platformVideoDownloadService.IsLikelyThrottled())
            {
                _logger.LogInformation("Skipping video download due to likely throttling");
                return;
            }

            var dataFetchDefinition = platformVideoDownloadService.DataFetchDefinition;

            var dataFetch = await _dataFetchService.AddDataFetchAsync(dataFetchDefinition, video, ct);

            var downloadTime = dataFetch.OccurredAt;
            try
            {
                var videoFilePath = await platformVideoDownloadService.DownloadVideoAsync(video, ct);

                dataFetch.Status = DataFetchStatus.Succeeded;
                dataFetch.DataFetchResults.Add(new DataFetchResult { Video = video, VideoId = video.Id });

                foreach (var videoFile in video.VideoFiles.AssertNotNull()
                             .Where(vf => vf.ValidUntil is null || vf.ValidUntil > downloadTime))
                {
                    videoFile.ValidUntil = downloadTime;
                }

                _dataUow.Ctx.VideoFiles.Add(new VideoFile
                {
                    FilePath = _appPaths.GetPathRelativeToDownloads(videoFilePath),
                    ValidSince = downloadTime, // Questionable semantics?
                    LastFetched = downloadTime,
                    Video = video,
                    VideoId = video.Id,
                });
            }
            catch (Exception e)
            {
                await _dataFetchService.UpdateDataFetchStatusAsync(dataFetch, DataFetchStatus.Failed,
                    message: e.ToString());
                return;
            }

            await _dataUow.SaveChangesAsync(CancellationToken.None); // Probably don't want to allow cancellation here because the video file is already downloaded
        }
        finally
        {
            if (!DownloadingVideoIds.TryRemove(video.Id, out _))
            {
                _logger.LogError("Video {VideoId} download wasn't tracked at end of download", video.Id);
            }
        }
    }
}