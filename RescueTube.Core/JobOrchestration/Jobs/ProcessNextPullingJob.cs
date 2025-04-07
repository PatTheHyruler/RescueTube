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
            .Select(j => (JobDefinition: j, Priority: _jobExecutionRegistry.GetJobPriority(j)))
            .Where(j => j.Priority > int.MinValue)
            .OrderByDescending(j => j.Priority)
            .Select(j => j.JobDefinition)
            .FirstOrDefault();
        return jobDefinition;
    }
}