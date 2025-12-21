using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RescueTube.Core.DataFetches;
using RescueTube.Core.JobOrchestration;

namespace RescueTube.Core.Jobs;

public class ManualDataFetchJob
{
    private readonly JobsConfiguration _config;
    private readonly IServiceProvider _serviceProvider;

    public ManualDataFetchJob(IOptions<JobsConfiguration> config, IServiceProvider serviceProvider)
    {
        _config = config.Value;
        _serviceProvider = serviceProvider;
    }

    [JobDisplayName("{0} for '{1}'")]
    [DisableConcurrentExecution("entity-{1}", timeoutSec: 5)]
    [Queue(JobQueues.HighPriority)]
    public async Task FetchEntityDataAsync(string jobId, Guid entityId, CancellationToken cancellationToken)
    {
        var jobDefinition = _config.RegisteredJobs.FirstOrDefault(x => x.JobId == jobId);
        if (jobDefinition is null)
        {
            throw new InvalidOperationException($"Job with id '{jobId}' not found");
        }

        var job = ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, jobDefinition.JobType);
        if (job is not IEntityDataFetchJob entityDataFetchJob)
        {
            throw new InvalidOperationException($"Job with id '{jobId}' is not an {nameof(IEntityDataFetchJob)}");
        }

        await entityDataFetchJob.FetchEntityDataAsync(entityId, cancellationToken);
    }
}