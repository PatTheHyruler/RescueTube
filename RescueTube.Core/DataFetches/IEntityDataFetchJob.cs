namespace RescueTube.Core.DataFetches;

public interface IEntityDataFetchJob
{
    public Task ExecuteEntityDataFetchAsync(Guid entityId, CancellationToken ct);
}