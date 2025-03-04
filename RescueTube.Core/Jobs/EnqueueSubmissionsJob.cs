using Hangfire;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Data;
using RescueTube.Core.Jobs.Filters;
using RescueTube.Core.Utils;

namespace RescueTube.Core.Jobs;

public class EnqueueSubmissionsJob
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly AppDbContext _dbContext;

    public EnqueueSubmissionsJob(IBackgroundJobClient backgroundJobClient, AppDbContext dbContext)
    {
        _backgroundJobClient = backgroundJobClient;
        _dbContext = dbContext;
    }

    [RescheduleConcurrentExecution("enqueue-submissions")]
    [Queue(JobQueues.HighPriority)]
    public async Task RunAsync(CancellationToken ct)
    {
        using var transaction = TransactionUtils.NewTransactionScope();
        var submissions = _dbContext.Submissions
            .Where(s => s.ApprovedAt != null && s.CompletedAt == null)
            .AsAsyncEnumerable().WithCancellation(ct);

        await foreach (var submission in submissions)
        {
            _backgroundJobClient.Enqueue<HandleSubmissionJob>(j => j.HandleSubmissionAsync(submission.Id, default));
        }
        transaction.Complete();
    }
}