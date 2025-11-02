using Hangfire.Server;

namespace RescueTube.Core.JobOrchestration;

public interface IJobBase
{
    public Task RunAsync(PerformContext performContext, CancellationToken ct);
}

public interface IJobWithId
{
    public static abstract string RecurringJobId { get; }
}

public interface IJob : IJobBase, IJobWithId;