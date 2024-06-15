using Hangfire;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Data;
using RescueTube.Domain.Enums;

namespace RescueTube.YouTube.Jobs;

public class EnqueueSubmissionsJob
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IAppDbContext _dbContext;

    public EnqueueSubmissionsJob(IBackgroundJobClient backgroundJobClient, IAppDbContext dbContext)
    {
        _backgroundJobClient = backgroundJobClient;
        _dbContext = dbContext;
    }

    [DisableConcurrentExecution(60 * 10)]
    public async Task RunAsync()
    {
        var submissions = _dbContext.Submissions
            .Where(s => s.ApprovedAt != null && s.CompletedAt == null && s.Platform == EPlatform.YouTube)
            .AsAsyncEnumerable();

        await foreach (var submission in submissions)
        {
            _backgroundJobClient.Enqueue<HandleSubmissionJob>(j => j.RunAsync(submission.Id, default));
        }
    }
}