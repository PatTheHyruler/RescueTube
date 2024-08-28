using RescueTube.Core.Data;
using RescueTube.Core.Jobs.Filters;
using RescueTube.Core.Utils;

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

    [SkipConcurrentSameArgExecution]
    public async Task RunAsync(Guid submissionId, CancellationToken ct = default)
    {
        using var transaction = TransactionUtils.NewTransactionScope();
        await _youTubeUow.SubmitService.HandleSubmissionAsync(submissionId, ct);
        await _dbContext.SaveChangesAsync(ct);
        transaction.Complete();
    }
}