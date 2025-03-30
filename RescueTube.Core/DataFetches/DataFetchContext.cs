using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace RescueTube.Core.DataFetches;

public class DataFetchContext
{
    private readonly TimeProvider _timeProvider;

    public readonly record struct DataFetchEntityDetail(string IdOnPlatform)
    {
        public static implicit operator DataFetchEntityDetail(string idOnPlatform) => new(idOnPlatform);
    }

    private readonly ConcurrentDictionary<
        DataFetchDefinition,
        ConcurrentDictionary<DataFetchEntityDetail, DateTimeOffset>
    > _ongoingGroupedDataFetches = [];

    public DataFetchContext(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    private bool TryStartDataFetch(DataFetchDefinition key, DataFetchEntityDetail entityDetail)
    {
        var dataFetches = _ongoingGroupedDataFetches.GetOrAdd(key, []);
        return dataFetches.TryAdd(entityDetail, _timeProvider.GetUtcNow());
    }

    public IDisposable? StartDataFetch(DataFetchDefinition key, DataFetchEntityDetail entityDetail, bool throwOnConflict = true)
    {
        var startedSuccessfully = TryStartDataFetch(key, entityDetail);
        if (startedSuccessfully)
        {
            return new DataFetchScope(this, key, entityDetail);
        }

        if (throwOnConflict)
        {
            throw new InvalidOperationException($"Data fetch {key} already ongoing for entity {entityDetail}");
        }

        return null;
    }

    private bool TryStopDataFetch(DataFetchDefinition key, DataFetchEntityDetail entityDetail)
    {
        return _ongoingGroupedDataFetches.TryGetValue(key, out var dataFetches)
               && dataFetches.TryRemove(entityDetail, out _);
    }

    public IReadOnlySet<string> GetCurrentlyFetchingEntityIdsOnPlatform(DataFetchDefinition key)
    {
        return _ongoingGroupedDataFetches.TryGetValue(key, out var dataFetches)
            ? dataFetches.Keys.Select(x => x.IdOnPlatform).ToImmutableHashSet()
            : ImmutableHashSet<string>.Empty;
    }

    public bool IsFetching(DataFetchDefinition key, string idOnPlatform)
    {
        return _ongoingGroupedDataFetches.TryGetValue(key, out var dataFetches)
               && dataFetches.ContainsKey(idOnPlatform);
    }

    private readonly struct DataFetchScope(DataFetchContext context, DataFetchDefinition key, DataFetchEntityDetail entityDetail) : IDisposable
    {
        public void Dispose()
        {
            var dataFetchStoppedSuccessfully = context.TryStopDataFetch(key, entityDetail);
            if (!dataFetchStoppedSuccessfully)
            {
                throw new InvalidOperationException($"Failed to close data fetch scope for fetch type {key}, entity {entityDetail}");
            }
        }
    }
}