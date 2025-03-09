using System.Collections.Concurrent;
using System.Collections.Immutable;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Constants.DataFetches;
using RescueTube.Core.Contracts;
using RescueTube.Core.Data;
using RescueTube.Core.Jobs.Filters;
using RescueTube.Core.Mediator;
using RescueTube.Core.Services;
using RescueTube.Core.Utils;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Jobs;

public class DownloadVideoJob
{
    private readonly ILogger<DownloadVideoJob> _logger;
    private readonly StorageLimitService _storageLimitService;
    private readonly IDataUow _dataUow;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeProvider _timeProvider;
    private readonly AppPaths _appPaths;
    private readonly IMediator _mediator;

    public DownloadVideoJob(ILogger<DownloadVideoJob> logger, StorageLimitService storageLimitService, IDataUow dataUow, IServiceProvider serviceProvider, TimeProvider timeProvider, AppPaths appPaths, IMediator mediator)
    {
        _logger = logger;
        _storageLimitService = storageLimitService;
        _dataUow = dataUow;
        _serviceProvider = serviceProvider;
        _timeProvider = timeProvider;
        _appPaths = appPaths;
        _mediator = mediator;
    }

    private static readonly ConcurrentDictionary<Guid, DateTimeOffset> DownloadingVideoIds = new();

    [AutomaticRetry(Attempts = 0)]
    [SkipConcurrent("core:download-not-downloaded-video-recurring")]
    [Queue(JobQueues.HighPriority)]
    public async Task DownloadNextNotDownloadedVideoAsync(CancellationToken ct)
    {
        if (await _storageLimitService.IsVideoDownloadForbiddenAsync(ct))
        {
            _logger.LogWarning("Skipping video download, storage limit reached.");
            return;
        }

        var downloadingVideoIds = DownloadingVideoIds.Keys.ToImmutableHashSet();
        var video = await _dataUow.Ctx.Videos
            .Where(v => v.VideoFiles!.Count == 0)
            .Where(v => v.DataFetches!
                .Where(d =>
                    d.Type == DataFetchTypes.VideoFileDownload)
                .OrderByDescending(d => d.OccurredAt)
                .Take(3)
                .Count(d => !d.Success) < 3)
            .Where(v => !downloadingVideoIds.Contains(v.Id))
            .Include(v => v.VideoFiles)
            .OrderBy(v => v.Id)
            .FirstOrDefaultAsync(ct);

        if (video is null)
        {
            return;
        }

        // TODO: Use DataFetchContext here instead?
        if (!DownloadingVideoIds.TryAdd(video.Id, _timeProvider.GetUtcNow()))
        {
            _logger.LogError("Video {VideoId} is already downloading.", video.Id);
            return;
        }

        try
        {
            var platformVideoDownloadService = _serviceProvider.GetRequiredKeyedService<IPlatformVideoDownloadService>(video.Platform);
            if (platformVideoDownloadService.IsLikelyThrottled())
            {
                _logger.LogInformation("Skipping video download due to likely throttling.");
                return;
            }

            var downloadTime = _timeProvider.GetUtcNow();
            try
            {
                var videoFilePath = await platformVideoDownloadService.DownloadVideoAsync(video, ct);

                _dataUow.Ctx.DataFetches.Add(new DataFetch
                {
                    Video = video,
                    VideoId = video.Id,
                    OccurredAt = downloadTime,
                    Success = true,
                    Type = platformVideoDownloadService.DataFetchDefinition.Type,
                    Source = platformVideoDownloadService.DataFetchDefinition.Source,
                    ShouldAffectValidity = false,
                });

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
                await _mediator.Send(new AddFailedDataFetchRequest
                {
                    VideoId = video.Id,
                    OccurredAt = downloadTime,
                    Message = e.Message,
                    Type = platformVideoDownloadService.DataFetchDefinition.Type,
                    Source = platformVideoDownloadService.DataFetchDefinition.Source,
                    ShouldAffectValidity = false,
                }, ct);
            }

            await _dataUow.SaveChangesAsync(CancellationToken.None); // Probably don't want to allow cancellation here because the video file is already downloaded
        }
        finally
        {
            if (!DownloadingVideoIds.TryRemove(video.Id, out _))
            {
                _logger.LogError("Video {VideoId} download wasn't tracked at end of download.", video.Id);
            }
        }
    }
}