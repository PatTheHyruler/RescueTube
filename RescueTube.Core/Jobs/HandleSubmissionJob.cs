using System.Collections.Concurrent;
using Hangfire;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Data;
using RescueTube.Core.JobOrchestration;
using RescueTube.Core.Services;

namespace RescueTube.Core.Jobs;

public sealed class HandleNextSubmissionJob(
    AppDbContext dbContext,
    SubmissionService submissionService,
    IBackgroundJobClientV2 backgroundJobClient,
    ILogger<HandleNextSubmissionJob> logger
    ) : IJob
{
    public static string RecurringJobId => "core:handle-submission";

    public static JobDefinition JobDefinition { get; } = new JobDefinition<HandleNextSubmissionJob>
    {
        IsArchivalJob = true,
        DefaultSettings = new()
        {
            JobId = RecurringJobId,
            Cron = "*/10 * * * *",
            IsEnabled = true,
        },
    };

    private static readonly ConcurrentDictionary<Guid, byte> SubmissionIdsBeingHandled = new();

    [Queue(JobQueues.Critical)]
    public async Task RunAsync(PerformContext performContext, CancellationToken ct)
    {
        var submissions = await dbContext.Submissions
            .Where(s => s.ApprovedAt != null && s.CompletedAt == null)
            .Where(s => !s.Failures!.Any())
            .Where(s => !SubmissionIdsBeingHandled.Keys.Contains(s.Id))
            .OrderBy(s => s.Id)
            .Take(2)
            .ToArrayAsync(ct);

        if (submissions is not [var submission, .. var nextSubmissions])
        {
            return;
        }

        var keySuccessfullyAdded = SubmissionIdsBeingHandled.TryAdd(submission.Id, byte.MinValue);
        if (keySuccessfullyAdded)
        {
            try
            {
                await submissionService.HandleSubmissionAsync(submission, ct);
            }
            finally
            {
                var keySuccessfullyRemoved = SubmissionIdsBeingHandled.TryRemove(submission.Id, out _);
                if (!keySuccessfullyRemoved)
                {
                    logger.LogWarning("Couldn't release submission {Id} after handling, it was already released", submission.Id);
                }
            }
        }
        else
        {
            logger.LogInformation("Fetched submission {Id}, but it was already being handled - skipping", submission.Id);
        }

        if (nextSubmissions.Length > 0)
        {
            backgroundJobClient.ContinueJobWith<IRecurringJobManagerV2>(performContext.BackgroundJob.Id,
                r => r.Trigger(RecurringJobId));
        }
    }
}