using Hangfire;
using RescueTube.Core.Data;
using RescueTube.Core.Jobs.Filters;
using RescueTube.Core.Services;
using RescueTube.Core.Utils;

namespace RescueTube.Core.Jobs;

public class HandleSubmissionJob
{
    private readonly AppDbContext _dbContext;
    private readonly SubmissionService _submissionService;

    public HandleSubmissionJob(AppDbContext dbContext, SubmissionService submissionService)
    {
        _dbContext = dbContext;
        _submissionService = submissionService;
    }

    [SkipConcurrent("handle-submission:{0}")]
    [Queue(JobQueues.Critical)]
    public async Task HandleSubmissionAsync(Guid submissionId, CancellationToken ct)
    {
        using var transaction = TransactionUtils.NewTransactionScope();
        await _submissionService.HandleSubmissionAsync(submissionId, ct);
        await _dbContext.SaveChangesAsync(ct);
        transaction.Complete();
    }
}