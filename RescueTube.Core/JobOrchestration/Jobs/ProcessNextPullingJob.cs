using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RescueTube.Core.Jobs.Filters;

namespace RescueTube.Core.JobOrchestration.Jobs;

public class ProcessNextPullingJob
{
    private readonly IOptions<JobsConfiguration> _config;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ProcessNextPullingJob> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly JobExecutionRegistry _jobExecutionRegistry;

    public ProcessNextPullingJob(IOptions<JobsConfiguration> config, TimeProvider timeProvider, ILogger<ProcessNextPullingJob> logger, IServiceProvider serviceProvider, JobExecutionRegistry jobExecutionRegistry)
    {
        _config = config;
        _timeProvider = timeProvider;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _jobExecutionRegistry = jobExecutionRegistry;
    }

    [SkipConcurrent("core:process-next-pulling-job:{0}")]
    [AutomaticRetry(Attempts = 0)]
    public async Task RunAsync(int workerIndex, CancellationToken ct)
    {
        var workerIsAlreadyRunning = !_jobExecutionRegistry.CurrentlyProcessingWorkers.TryAdd(workerIndex, _timeProvider.GetUtcNow());
        if (workerIsAlreadyRunning)
        {
            _logger.LogError("Worker with index {WorkerIndex} was already running", workerIndex);
            return;
        }
        try
        {
            await ProcessNextJobAsync(ct);
        }
        finally
        {
            var workerWasNotRunning = !_jobExecutionRegistry.CurrentlyProcessingWorkers.TryRemove(workerIndex, out _);
            if (workerWasNotRunning)
            {
                _logger.LogError("Worker with index {WorkerIndex} was already not running when finishing up", workerIndex);
            }
        }
    }

    private async Task ProcessNextJobAsync(CancellationToken ct)
    {
        var jobDefinition = GetNextJobDefinition();
        if (jobDefinition is null)
        {
            _logger.LogDebug("No next job definition found, skipping");
            return;
        }

        var jobId = Guid.CreateVersion7();
        try
        {
            // Note that because we add the job to StartedJobs after the check in GetNextJobDefinition,
            // it is possible GetNextJobDefinition will return the same job again in another thread.
            // This should be fine, for now at least.
            _jobExecutionRegistry.StartedJobs.GetOrAdd(jobDefinition, [])[jobId] = new JobInvocationInfo(_timeProvider.GetUtcNow());

            var job = jobDefinition.GetJob(_serviceProvider);
            _logger.LogInformation("Executing job {JobType}", job.GetType().FullName);
            var result = await job.RunAsync(ct);
            _jobExecutionRegistry.StartedJobs[jobDefinition][jobId].MarkFinished(_timeProvider.GetUtcNow(), result);
        }
        catch
        {
            _jobExecutionRegistry.StartedJobs[jobDefinition][jobId].MarkFinished(_timeProvider.GetUtcNow(), JobExecutionResult.Errored);
        }
    }

    private JobDefinition? GetNextJobDefinition()
    {
        var jobDefinition = _config.Value.RegisteredJobs
            .Select(j => (JobDefinition: j, Priority: GetJobPriority(j)))
            .Where(j => j.Priority > int.MinValue)
            .OrderByDescending(j => j.Priority)
            .Select(j => j.JobDefinition)
            .FirstOrDefault();
        return jobDefinition;
    }

    private int GetJobPriority(JobDefinition jobDefinition)
    {
        var invocations = _jobExecutionRegistry.StartedJobs.GetOrAdd(jobDefinition, []).Values;
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