using RescueTube.Core.Utils;

namespace RescueTube.Core.JobOrchestration.Jobs;

public class ClearOldJobExecutionStatsJob
{
    private readonly JobExecutionRegistry _jobExecutionRegistry;
    private readonly TimeProvider _timeProvider;

    public ClearOldJobExecutionStatsJob(JobExecutionRegistry jobExecutionRegistry, TimeProvider timeProvider)
    {
        _jobExecutionRegistry = jobExecutionRegistry;
        _timeProvider = timeProvider;
    }

    public void Run(CancellationToken ct)
    {
        foreach (var invocations in _jobExecutionRegistry.StartedJobs.Values.TakeWhileNotCancelled(ct))
        {
            var oldInvocations = invocations.Where(i => i.Value.FinishedAt < _timeProvider.GetUtcNow().AddMinutes(-15));
            foreach (var oldInvocation in oldInvocations.TakeWhileNotCancelled(ct))
            {
                invocations.TryRemove(oldInvocation);
            }
        }
    }
}