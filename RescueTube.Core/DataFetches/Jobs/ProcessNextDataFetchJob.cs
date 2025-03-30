using System.Linq.Expressions;
using Hangfire;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RescueTube.Core.Data;
using RescueTube.Core.Jobs.Filters;
using RescueTube.Domain.Contracts;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.DataFetches.Jobs;

public class ProcessNextDataFetchJob
{
    private readonly IOptions<DataFetchJobsConfiguration> _config;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ProcessNextDataFetchJob> _logger;
    private readonly IDataUow _dataUow;
    private readonly IServiceProvider _serviceProvider;
    private readonly DataFetchJobContext _dataFetchJobContext;

    public ProcessNextDataFetchJob(IOptions<DataFetchJobsConfiguration> config, TimeProvider timeProvider, ILogger<ProcessNextDataFetchJob> logger, IDataUow dataUow, IServiceProvider serviceProvider, DataFetchJobContext dataFetchJobContext)
    {
        _config = config;
        _timeProvider = timeProvider;
        _logger = logger;
        _dataUow = dataUow;
        _serviceProvider = serviceProvider;
        _dataFetchJobContext = dataFetchJobContext;
    }

    [SkipConcurrent("core:process-next-data-fetch:{0}")]
    [AutomaticRetry(Attempts = 0)]
    public async Task RunAsync(int workerIndex, CancellationToken ct)
    {
        var workerIsAlreadyRunning = !_dataFetchJobContext.CurrentlyProcessingWorkers.TryAdd(workerIndex, _timeProvider.GetUtcNow());
        if (workerIsAlreadyRunning)
        {
            _logger.LogError("Worker with index {WorkerIndex} was already running", workerIndex);
            return;
        }
        try
        {
            await ProcessNextDataFetchAsync(ct);
        }
        finally
        {
            var workerWasNotRunning = !_dataFetchJobContext.CurrentlyProcessingWorkers.TryRemove(workerIndex, out _);
            if (workerWasNotRunning)
            {
                _logger.LogError("Worker with index {WorkerIndex} was already not running when finishing up", workerIndex);
            }
        }
    }

    private async Task ProcessNextDataFetchAsync(CancellationToken ct)
    {
        var nextJobDefinition = GetNextJobDefinition();
        if (nextJobDefinition is null)
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
            _dataFetchJobContext.StartedJobs.GetOrAdd(nextJobDefinition.DataFetchDefinition, [])[jobId] = new JobInvocationInfo(_timeProvider.GetUtcNow());
            using var logScope = _logger.BeginScope(
                "Processing data fetch job {Platform}, {EntityType}, {Source}, {Type}",
                nextJobDefinition.DataFetchDefinition.Platform,
                nextJobDefinition.DataFetchDefinition.EntityType,
                nextJobDefinition.DataFetchDefinition.Source,
                nextJobDefinition.DataFetchDefinition.Type);
            await ExecuteDataFetchJobAsync(nextJobDefinition, ct);
        }
        finally
        {
            _dataFetchJobContext.StartedJobs[nextJobDefinition.DataFetchDefinition][jobId].MarkFinished(_timeProvider.GetUtcNow());
        }
    }

    private DataFetchJobDefinition? GetNextJobDefinition()
    {
        var jobDefinition = _config.Value.RegisteredJobs
            .Select(j => (JobDefinition: j, Priority: GetJobPriority(j)))
            .Where(j => j.Priority > int.MinValue)
            .OrderByDescending(j => j.Priority)
            .Select(j => j.JobDefinition)
            .FirstOrDefault();
        return jobDefinition;
    }

    private int GetJobPriority(DataFetchJobDefinition jobDefinition)
    {
        var invocations = _dataFetchJobContext.StartedJobs.GetOrAdd(jobDefinition.DataFetchDefinition, []);
        var runningInvocationsCount = invocations.Values.Count(i => i.IsRunning);

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

        var now = _timeProvider.GetUtcNow();
        var resourceUsageTimespan = TimeSpan.FromMinutes(5);
        var timeUsageFairnessCutoff = now.Subtract(resourceUsageTimespan);
        var resourceUsageStatistics = invocations.Values
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

    private Task ExecuteDataFetchJobAsync(DataFetchJobDefinition jobDefinition, CancellationToken ct)
    {
#pragma warning disable CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
        return jobDefinition.DataFetchDefinition.EntityType switch
#pragma warning restore CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
        {
            EEntityType.Video => ExecuteDataFetchJobAsync(jobDefinition, _dataUow.DataFetches.ShouldFetchData<Domain.Entities.Video>(jobDefinition), ct),
            EEntityType.Author => ExecuteDataFetchJobAsync(jobDefinition, _dataUow.DataFetches.ShouldFetchAuthorData(jobDefinition), ct),
            EEntityType.Playlist => ExecuteDataFetchJobAsync(jobDefinition, _dataUow.DataFetches.ShouldFetchData<Domain.Entities.Playlist>(jobDefinition), ct),
        };
    }

    private async Task ExecuteDataFetchJobAsync<TEntity>(
        DataFetchJobDefinition jobDefinition,
        Expression<Func<TEntity, bool>> filterExpression,
        CancellationToken ct
    ) where TEntity : class, IIdDatabaseEntity, IPlatformEntity, IFetchable
    {
        var entityId = await _dataUow.Ctx.Set<TEntity>()
            .AsExpandable()
            .Where(filterExpression)
            .OrderBy(x => x.Id)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(ct);
        if (entityId == Guid.Empty)
        {
            return;
        }

        using var logScope = _logger.BeginScope("Fetching data for {EntityId}", entityId);
        _logger.LogInformation("Starting data fetch, {Platform}, {Type}, {Source}, {EntityType}, {EntityId}",
            jobDefinition.DataFetchDefinition.Platform,
            jobDefinition.DataFetchDefinition.Type,
            jobDefinition.DataFetchDefinition.Source,
            jobDefinition.DataFetchDefinition.EntityType,
            entityId);
        await jobDefinition.GetDataFetchJob(_serviceProvider).ExecuteEntityDataFetchAsync(entityId, ct);
    }
}