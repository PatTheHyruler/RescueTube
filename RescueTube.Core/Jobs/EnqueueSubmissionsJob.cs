using Hangfire;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Data;
using RescueTube.Core.JobOrchestration;
using RescueTube.Core.Jobs.Filters;

namespace RescueTube.Core.Jobs;

public class EnqueueSubmissionsJob : IJob
{
    public static string RecurringJobId => "core:enqueue-submissions";

    public static JobDefinition JobDefinition { get; } = new JobDefinition<EnqueueSubmissionsJob>
    {
        IsArchivalJob = true,
        DefaultSettings = new()
        {
            JobId = RecurringJobId,
            Cron = Cron.Hourly(),
            IsEnabled = true,
        },
    };

    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly AppDbContext _dbContext;

    public EnqueueSubmissionsJob(IBackgroundJobClient backgroundJobClient, AppDbContext dbContext)
    {
        _backgroundJobClient = backgroundJobClient;
        _dbContext = dbContext;
    }

    [RescheduleConcurrentExecution("enqueue-submissions")]
    [Queue(JobQueues.HighPriority)]
    public async Task RunAsync(PerformContext performContext, CancellationToken ct)
    {
        var submissions = _dbContext.Submissions
            .Where(s => s.ApprovedAt != null && s.CompletedAt == null)
            .AsAsyncEnumerable().WithCancellation(ct);

        await foreach (var submission in submissions)
        {
            _backgroundJobClient.Enqueue<HandleSubmissionJob>(j => j.HandleSubmissionAsync(submission.Id, default));
        }
    }
}