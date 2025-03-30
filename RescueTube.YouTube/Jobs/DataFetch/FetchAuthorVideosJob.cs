using RescueTube.Core.Data;
using RescueTube.Core.DataFetches;
using RescueTube.Core.Jobs;
using RescueTube.Core.Utils;

namespace RescueTube.YouTube.Jobs.DataFetch;

public class FetchAuthorVideosJob : IEntityDataFetchJob
{
    private readonly IDataUow _dataUow;
    private readonly YouTubeUow _youTubeUow;

    public FetchAuthorVideosJob(IDataUow dataUow, YouTubeUow youTubeUow)
    {
        _dataUow = dataUow;
        _youTubeUow = youTubeUow;
    }

    public async Task ExecuteEntityDataFetchAsync(Guid entityId, CancellationToken ct)
    {
        using var transaction = TransactionUtils.NewTransactionScope();
        await _youTubeUow.AuthorService.TryFetchAuthorVideosAsync(authorId: entityId, force: false, ct: ct);
        await _dataUow.SaveChangesAsync(ct);
        transaction.Complete();
    }
}