using Hangfire;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Data;
using RescueTube.Core.JobOrchestration;
using RescueTube.Core.Jobs.Filters;
using RescueTube.Core.Services;

namespace RescueTube.Core.Jobs;

public class UpdateImagesResolutionJob : IJob
{
    public const string RecurringJobId = "core:update-images-resolution-from-file";
    static string IJobWithId.RecurringJobId => RecurringJobId;

    private readonly IDataUow _dataUow;
    private readonly ImageService _imageService;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public UpdateImagesResolutionJob(IDataUow dataUow, ImageService imageService,
        IBackgroundJobClient backgroundJobClient)
    {
        _dataUow = dataUow;
        _imageService = imageService;
        _backgroundJobClient = backgroundJobClient;
    }

    [SkipConcurrent(RecurringJobId)]
    [Queue(JobQueues.LowerPriority)]
    public async Task RunAsync(PerformContext performContext, CancellationToken ct)
    {
        var images = await _dataUow.Ctx.Images
            .Where(_dataUow.Images.ShouldAttemptResolutionUpdate)
            .Take(100)
            .ToArrayAsync(ct);

        if (images.Length == 0)
        {
            return;
        }

        foreach (var imageId in images)
        {
            await _imageService.TryUpdateResolutionFromFileAsync(imageId, ct);
        }

        await _dataUow.SaveChangesAsync(ct);

        _backgroundJobClient.ContinueJobWith<IRecurringJobManagerV2>(performContext.BackgroundJob.Id,
            r => r.Trigger(RecurringJobId));
    }
}