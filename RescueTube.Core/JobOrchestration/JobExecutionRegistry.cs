using System.Collections.Concurrent;

namespace RescueTube.Core.JobOrchestration;

public class JobExecutionRegistry
{
    private readonly TimeProvider _timeProvider;

    public JobExecutionRegistry(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public ConcurrentDictionary<
        JobDefinition,
        ConcurrentDictionary<Guid, JobInvocationInfo>
    > StartedJobs { get; } = [];

    public ConcurrentDictionary<int, DateTimeOffset> CurrentlyProcessingWorkers { get; } = [];

    public int GetJobPriority(JobDefinition jobDefinition)
    {
        var invocations = StartedJobs.GetOrAdd(jobDefinition, []).Values;
        var runningInvocationsCount = invocations.Count(i => i.IsRunning);

        var maxInvocationsRank = jobDefinition.PreferredMaxConcurrentExecutions - runningInvocationsCount;
        if (maxInvocationsRank <= 0)
        {
            return int.MinValue;
        }

        var result = maxInvocationsRank;

        var minInvocationsRank = jobDefinition.PreferredMinConcurrentExecutions - runningInvocationsCount;
        if (minInvocationsRank > 0)
        {
            result += minInvocationsRank;
        }

        result += jobDefinition.Priority * 64;

        var erroredCount = invocations.Count(x => x is { IsRunning: false, Result: JobExecutionResult.Errored });
        var totalCount = invocations.Count(x => !x.IsRunning);
        if (totalCount > 0 && (float)erroredCount / totalCount > 0.9)
        {
            return int.MinValue;
        }

        var previousExecutionInfo = invocations
            .Where(x => !x.IsRunning)
            .OrderByDescending(x => x.FinishedAt)
            .FirstOrDefault();
        if (previousExecutionInfo is not null)
        {
            switch (previousExecutionInfo.Result)
            {
                case JobExecutionResult.NothingToProcess
                    when previousExecutionInfo.FinishedAt > _timeProvider.GetUtcNow().AddSeconds(-60):
                    return int.MinValue;
                case JobExecutionResult.HasMoreToProcess:
                    result += 64;
                    break;
            }
        }

        var now = _timeProvider.GetUtcNow();
        var resourceUsageTimespan = TimeSpan.FromMinutes(5);
        var timeUsageFairnessCutoff = now.Subtract(resourceUsageTimespan);
        var resourceUsageStatistics = invocations
            .Where(i => i.StartedAt > timeUsageFairnessCutoff)
            .Select(i => (i.FinishedAt ?? now) - i.StartedAt)
            .Aggregate((UsedTime: TimeSpan.Zero, Count: 0),
                (previous, i) => (
                    previous.UsedTime + i,
                    previous.Count + 1));
        result -= resourceUsageStatistics.Count / 4;
        var usedTimePercentage = resourceUsageStatistics.UsedTime.TotalSeconds / resourceUsageTimespan.TotalSeconds;
        var timeFactorToSubtract = result * usedTimePercentage / (1 + usedTimePercentage);
        result -= (int)timeFactorToSubtract;

        return result;
    }
}