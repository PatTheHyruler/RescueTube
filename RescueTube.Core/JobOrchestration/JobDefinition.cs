using RescueTube.Core.DataFetches;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.JobOrchestration;

public abstract record JobDefinition
{
    public required JobSettings DefaultSettings { get; init; }

    public abstract string JobId { get; }

    public abstract Hangfire.Common.Job CreateHangfireJob();
    public Hangfire.RecurringJobOptions HangfireRecurringJobOptions { get; } = new()
    {
        MisfireHandling = Hangfire.MisfireHandlingMode.Relaxed,        
    };

    public required bool IsArchivalJob { get; init; }

    public abstract Type JobType { get; }

    public DataFetchDefinition? DataFetchDefinition { get; init; }
}

public record JobDefinition<TJob> : JobDefinition where TJob : IJob
{
    public override string JobId => TJob.RecurringJobId;

    public override Hangfire.Common.Job CreateHangfireJob()
    {
        // TODO: Specify queue
        return Hangfire.Common.Job.FromExpression<TJob>(j => j.RunAsync(null!, CancellationToken.None));
    }

    public override Type JobType => typeof(TJob);
}