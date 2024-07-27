using RescueTube.Core.Data;
using RescueTube.Core.Jobs.Filters;

namespace RescueTube.YouTube.Jobs;

public class HandleSubmissionJob
{
    private readonly AppDbContext _dbContext;
    private readonly YouTubeUow _youTubeUow;

    public HandleSubmissionJob(AppDbContext dbContext, YouTubeUow youTubeUow)
    {
        _dbContext = dbContext;
        _youTubeUow = youTubeUow;
    }

    [DisableConcurrentSameArgExecution(60)]
    public async Task RunAsync(Guid submissionId, CancellationToken ct = default)
    {
        await _youTubeUow.SubmitService.HandleSubmissionAsync(submissionId, ct);
        await _dbContext.SaveChangesAsync(ct);
    }
}