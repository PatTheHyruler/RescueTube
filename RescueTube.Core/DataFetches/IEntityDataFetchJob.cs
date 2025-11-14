namespace RescueTube.Core.DataFetches;

public interface IEntityDataFetchJob
{
    Task<EntityDataFetchResult> FetchEntityDataAsync(Guid entityId, CancellationToken ct);
}

/// <remarks>This is a separate interface from <see cref="IEntityDataFetchJob"/> only because static abstract interface members don't work well alongside abstract classes.</remarks>
public interface IEntityDataFetchJobWithDefinition : IEntityDataFetchJob
{
    static abstract DataFetchJobDefinition DataFetchJobDefinition { get; }
}