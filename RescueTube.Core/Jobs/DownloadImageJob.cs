using System.Collections.Concurrent;
using Hangfire;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Data;
using RescueTube.Core.JobOrchestration;
using RescueTube.Core.Jobs.Filters;
using RescueTube.Core.Services;

namespace RescueTube.Core.Jobs;

public class DownloadImageJob : IJob
{
    public static string RecurringJobId => "core:download-image";

    public static readonly JobDefinition<DownloadImageJob> JobDefinition = new()
    {
        IsArchivalJob = true,
        DefaultSettings = new()
        {
            JobId = RecurringJobId,
            Cron = Cron.Minutely(),
            IsEnabled = true,
        },
    };

    private readonly ImageService _imageService;
    private readonly IDataUow _dataUow;
    private readonly IBackgroundJobClientV2 _backgroundJobClient;

    public DownloadImageJob(ImageService imageService, IDataUow dataUow, IBackgroundJobClientV2 backgroundJobClient)
    {
        _imageService = imageService;
        _dataUow = dataUow;
        _backgroundJobClient = backgroundJobClient;
    }

    private static readonly ConcurrentDictionary<Guid, bool> CurrentlyProcessingImageIds = [];

    [SkipConcurrent(resource: "core:download-image")]
    [Queue(JobQueues.LowPriority)]
    public async Task RunAsync(PerformContext performContext, CancellationToken ct)
    {
        var images = await _dataUow.Ctx.Images
            .Where(i =>
                    i.LocalFilePath == null
                    && i.FailedFetchAttempts < 3
                    && i.Url != null)
            .Where(i => !CurrentlyProcessingImageIds.Keys.Contains(i.Id))
            // TODO: Filter by entity image validity and entity settings?
            .Include(i => i.AuthorImages)
            .Include(i => i.VideoImages)
            .Include(i => i.PlaylistImages)
            .AsSingleQuery()
            .Take(2)
            .ToArrayAsync(ct);

        if (images is not [var image, .. var nextImages])
        {
            return;
        }

        if (CurrentlyProcessingImageIds.TryAdd(image.Id, false))
        {
            try
            {
                await _imageService.DownloadImageAsync(image, ct);
                await _dataUow.SaveChangesAsync(ct);
            }
            finally
            {
                CurrentlyProcessingImageIds.TryRemove(image.Id, out _);
            }
        }

        if (nextImages.Length > 0)
        {
            _backgroundJobClient.ContinueJobWith<IRecurringJobManagerV2>(performContext.BackgroundJob.Id,
                r => r.TriggerJob(RecurringJobId));
        }
    }
}