using RescueTube.Core.Data;
using RescueTube.Core.DataFetches;
using RescueTube.Core.Utils;

namespace RescueTube.YouTube.Jobs.DataFetch;

public class FetchVideoDataJob : IEntityDataFetchJob
{
    private readonly IDataUow _dataUow;
    private readonly YouTubeUow _youTubeUow;

    public FetchVideoDataJob(IDataUow dataUow, YouTubeUow youTubeUow)
    {
        _dataUow = dataUow;
        _youTubeUow = youTubeUow;
    }

    public async Task ExecuteEntityDataFetchAsync(Guid entityId, CancellationToken ct)
    {
        using var transaction = TransactionUtils.NewTransactionScope();
        await _youTubeUow.VideoService.UpdateVideoAsync(entityId, ct);
        await _dataUow.SaveChangesAsync(ct);
        transaction.Complete();
    }
}