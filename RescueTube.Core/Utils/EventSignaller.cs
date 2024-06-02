namespace RescueTube.Core.Utils;

public class EventSignaller : IDisposable
{
    private CancellationTokenSource _cancellationTokenSource = new();

    public async Task Delay(TimeSpan delay, params CancellationToken[] cancellationTokens)
    {
        CancellationToken ct;
        CancellationTokenSource? linkedTokenSource = null;
        if (cancellationTokens.Length == 0)
        {
            ct = _cancellationTokenSource.Token;
        }
        else
        {
            var combinedTokens = new CancellationToken[cancellationTokens.Length + 1];
            combinedTokens[0] = _cancellationTokenSource.Token;
            cancellationTokens.CopyTo(combinedTokens, 1);
            linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(combinedTokens);
            ct = linkedTokenSource.Token;
        }

        await Task.Delay(delay, ct).ContinueWith(_ => { }, CancellationToken.None);  // TODO: Figure out better way to do this?

        linkedTokenSource?.Dispose();
    }

    public Task Delay(int millisecondsDelay, params CancellationToken[] cancellationTokens) =>
        Delay(TimeSpan.FromMilliseconds(millisecondsDelay), cancellationTokens);

    public void Signal()
    {
        lock (_cancellationTokenSource)
        {
            if (_cancellationTokenSource.IsCancellationRequested) return;
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource.Dispose();
    }
}