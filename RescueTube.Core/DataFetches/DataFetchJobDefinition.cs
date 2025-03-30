using Microsoft.Extensions.DependencyInjection;

namespace RescueTube.Core.DataFetches;

public abstract record DataFetchJobDefinition
{
    public required DataFetchDefinition DataFetchDefinition { get; init; }

    public required TimeSpan SuccessCutoffOffset { get; init; }
    public required TimeSpan FailureCutoffOffset { get; init; }

    public int PreferredMinConcurrentExecutions { get; init; } = 0;
    public int PreferredMaxConcurrentExecutions { get; init; } = 1;

    public int Priority { get; init; }

    public abstract IEntityDataFetchJob GetDataFetchJob(IServiceProvider serviceProvider);
}

public record DataFetchJobDefinition<TDataFetchJob> : DataFetchJobDefinition where TDataFetchJob : IEntityDataFetchJob
{
    public override IEntityDataFetchJob GetDataFetchJob(IServiceProvider serviceProvider)
    {
        return ActivatorUtilities.GetServiceOrCreateInstance<TDataFetchJob>(serviceProvider);
    }
}