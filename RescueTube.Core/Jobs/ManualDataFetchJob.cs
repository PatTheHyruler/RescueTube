using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RescueTube.Core.DataFetches;

namespace RescueTube.Core.Jobs;

public class ManualDataFetchJob
{
    private readonly IOptions<DataFetchJobsConfiguration> _config;
    private readonly IServiceProvider _serviceProvider;

    public ManualDataFetchJob(IOptions<DataFetchJobsConfiguration> config, IServiceProvider serviceProvider)
    {
        _config = config;
        _serviceProvider = serviceProvider;
    }

    [JobDisplayName("{0} for '{1}'")]
    [DisableConcurrentExecution("entity-{1}", timeoutSec: 5)]
    public async Task FetchEntityDataAsync(string jobName, Guid entityId, CancellationToken cancellationToken)
    {
        var jobDefinition = _config.Value.RegisteredJobs.FirstOrDefault(x => x.JobName == jobName);
        if (jobDefinition is null)
        {
            throw new InvalidOperationException($"Job with name '{jobName}' not found");
        }

        var job = ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, jobDefinition.JobType);
        if (job is not IEntityDataFetchJob entityDataFetchJob)
        {
            throw new InvalidOperationException($"Job with name '{jobName}' is not an {nameof(IEntityDataFetchJob)}");
        }

        await entityDataFetchJob.FetchEntityDataAsync(entityId, cancellationToken);
    }
}