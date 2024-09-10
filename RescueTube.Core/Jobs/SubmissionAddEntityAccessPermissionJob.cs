using Hangfire;
using RescueTube.Core.Services;
using RescueTube.Core.Utils;

namespace RescueTube.Core.Jobs;

public class SubmissionAddEntityAccessPermissionJob
{
    private readonly SubmissionService _submissionService;

    public SubmissionAddEntityAccessPermissionJob(SubmissionService submissionService)
    {
        _submissionService = submissionService;
    }

    [Queue(JobQueues.HighPriority)]
    public async Task RunAsync(Guid submissionId, CancellationToken ct = default)
    {
        using var transaction = TransactionUtils.NewTransactionScope();
        await _submissionService.SubmissionAddEntityAccessPermissionAsync(submissionId, ct);
        await _submissionService.ServiceUow.SaveChangesAsync(ct);
        transaction.Complete();
    }
}