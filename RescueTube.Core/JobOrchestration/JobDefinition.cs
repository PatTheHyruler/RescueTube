using RescueTube.Domain.Entities;

namespace RescueTube.Core.JobOrchestration;

public abstract record JobDefinition
{
    protected JobDefinition(Type jobType)
    {
        JobType = jobType;
    }

    public Type JobType { get; }
    public string? Name => JobType.FullName;

    public required JobSettings DefaultSettings { get; init; }

    public abstract string JobId { get; }

    public abstract Hangfire.Common.Job CreateHangfireJob();
    public Hangfire.RecurringJobOptions HangfireRecurringJobOptions { get; } = new()
    {
        MisfireHandling = Hangfire.MisfireHandlingMode.Relaxed,        
    };

    public required bool IsArchivalJob { get; init; }

    public int PreferredMaxConcurrentExecutions { get; init; } = 1;

    public int Priority { get; init; }
}

public record JobDefinition<TJob> : JobDefinition where TJob : IJob
{
    public JobDefinition() : base(typeof(TJob))
    {
    }

    public override string JobId => TJob.RecurringJobId;

    public override Hangfire.Common.Job CreateHangfireJob()
    {
        // TODO: Specify queue
        return Hangfire.Common.Job.FromExpression<TJob>(j => j.RunAsync(null!, CancellationToken.None));
    }
}