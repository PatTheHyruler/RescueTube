using Hangfire;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Data;
using RescueTube.Core.Utils;
using RescueTube.Domain.Enums;

namespace RescueTube.YouTube.Jobs;

public class EnqueueSubmissionsJob
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly AppDbContext _dbContext;

    public EnqueueSubmissionsJob(IBackgroundJobClient backgroundJobClient, AppDbContext dbContext)
    {
        _backgroundJobClient = backgroundJobClient;
        _dbContext = dbContext;
    }

    [DisableConcurrentExecution(60 * 10)]
    public async Task RunAsync(CancellationToken ct)
    {
        using var transaction = TransactionUtils.NewTransactionScope();
        var submissions = _dbContext.Submissions
            .Where(s => s.ApprovedAt != null && s.CompletedAt == null && s.Platform == EPlatform.YouTube)
            .AsAsyncEnumerable().WithCancellation(ct);

        await foreach (var submission in submissions)
        {
            _backgroundJobClient.Enqueue<HandleSubmissionJob>(j => j.RunAsync(submission.Id, default));
        }
        transaction.Complete();
    }
}