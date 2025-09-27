using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.DataFetches;

public sealed class DataFetchScope : IAsyncDisposable
{
    private readonly DataFetchService _dataFetchService;
    public DataFetch DataFetch { get; }

    public DataFetchScope(DataFetch dataFetch, DataFetchService dataFetchService)
    {
        DataFetch = dataFetch;
        _dataFetchService = dataFetchService;
    }

    public async ValueTask DisposeAsync()
    {
        if (DataFetch.Status is DataFetchStatus.Starting)
        {
            await _dataFetchService.UpdateDataFetchStatusAsync(DataFetch, DataFetchStatus.Failed, $"DataFetchScope disposed while DataFetch status was {DataFetch.Status}");
        }
    }
}