using RescueTube.Core.Services;

namespace RescueTube.Core.Jobs;

public class SubmissionAddEntityAccessPermissionJob
{
    private readonly SubmissionService _submissionService;

    public SubmissionAddEntityAccessPermissionJob(SubmissionService submissionService)
    {
        _submissionService = submissionService;
    }

    public async Task RunAsync(Guid submissionId, CancellationToken ct = default)
    {
        await _submissionService.SubmissionAddEntityAccessPermissionAsync(submissionId, ct);
        await _submissionService.ServiceUow.SaveChangesAsync(ct);
    }
}