using Microsoft.Extensions.DependencyInjection;

namespace RescueTube.Core.JobOrchestration;

public abstract record JobDefinition
{
    public Guid Id { get; } = Guid.NewGuid();

    public string? Name { get; init; }

    public int PreferredMinConcurrentExecutions { get; init; } = 0;
    public int PreferredMaxConcurrentExecutions { get; init; } = 1;

    public int Priority { get; init; }

    public abstract IJob GetJob(IServiceProvider serviceProvider);
}

public record JobDefinition<TJob> : JobDefinition where TJob : IJob
{
    public JobDefinition()
    {
        Name = typeof(TJob).FullName;
    }

    public override IJob GetJob(IServiceProvider serviceProvider)
    {
        return ActivatorUtilities.GetServiceOrCreateInstance<TJob>(serviceProvider);
    }
}