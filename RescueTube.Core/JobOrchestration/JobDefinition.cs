using Microsoft.Extensions.DependencyInjection;

namespace RescueTube.Core.JobOrchestration;

public abstract record JobDefinition
{
    protected JobDefinition(Type jobType)
    {
        JobType = jobType;
    }

    public Guid Id { get; } = Guid.NewGuid();

    public Type JobType { get; }
    public string? Name => JobType.FullName;

    public int PreferredMinConcurrentExecutions { get; init; } = 0;
    public int PreferredMaxConcurrentExecutions { get; init; } = 1;

    public int Priority { get; init; }

    public abstract IJob GetJob(IServiceProvider serviceProvider);
}

public record JobDefinition<TJob> : JobDefinition where TJob : IJob
{
    public JobDefinition() : base(typeof(TJob))
    {
    }

    public override IJob GetJob(IServiceProvider serviceProvider)
    {
        return ActivatorUtilities.GetServiceOrCreateInstance<TJob>(serviceProvider);
    }
}