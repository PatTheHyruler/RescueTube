namespace RescueTube.Core.DataFetches;

public interface IEntityDataFetchJob
{
    Task<EntityDataFetchResult> FetchEntityDataAsync(Guid entityId, CancellationToken ct);
}