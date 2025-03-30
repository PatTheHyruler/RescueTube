using RescueTube.Core.Utils;

namespace RescueTube.Core.DataFetches.Jobs;

public class ClearOldDataFetchContextEntriesJob
{
    private readonly DataFetchJobContext _dataFetchJobContext;
    private readonly TimeProvider _timeProvider;

    public ClearOldDataFetchContextEntriesJob(DataFetchJobContext dataFetchJobContext, TimeProvider timeProvider)
    {
        _dataFetchJobContext = dataFetchJobContext;
        _timeProvider = timeProvider;
    }

    public void Run(CancellationToken ct)
    {
        foreach (var invocations in _dataFetchJobContext.StartedJobs.Values.TakeWhileNotCancelled(ct))
        {
            var oldInvocations = invocations.Where(i => i.Value.FinishedAt < _timeProvider.GetUtcNow().AddMinutes(-15));
            foreach (var oldInvocation in oldInvocations.TakeWhileNotCancelled(ct))
            {
                invocations.TryRemove(oldInvocation);
            }
        }
    }
}