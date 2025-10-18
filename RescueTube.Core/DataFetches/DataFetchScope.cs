using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.DataFetches;

public sealed class DataFetchScope : IAsyncDisposable
{
    private readonly DataFetchService _dataFetchService;

    private readonly CancellationTokenSource _cancellationTokenSource;

    public DataFetch DataFetch { get; }

    public DataFetchScope(DataFetch dataFetch, DataFetchService dataFetchService, CancellationToken ct)
    {
        DataFetch = dataFetch;
        _dataFetchService = dataFetchService;
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(ct);
        Task.Run(async () =>
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    await _dataFetchService.SendDataFetchHeartbeatAsync(dataFetch, ct);
                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        break;
                    }
                    await Task.Delay(TimeSpan.FromMinutes(1), ct);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }, _cancellationTokenSource.Token);
    }

    public async ValueTask DisposeAsync()
    {
        if (!_cancellationTokenSource.IsCancellationRequested)
        {
            await _cancellationTokenSource.CancelAsync();
        }

        if (DataFetch.Status is DataFetchStatus.Started)
        {
            await _dataFetchService.UpdateDataFetchStatusAsync(DataFetch, DataFetchStatus.Failed, $"DataFetchScope disposed while DataFetch status was {DataFetch.Status}");
        }
    }
}