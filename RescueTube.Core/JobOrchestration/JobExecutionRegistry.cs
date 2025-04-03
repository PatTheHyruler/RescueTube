using System.Collections.Concurrent;

namespace RescueTube.Core.JobOrchestration;

public class JobExecutionRegistry
{
    public ConcurrentDictionary<
        JobDefinition,
        ConcurrentDictionary<Guid, JobInvocationInfo>
    > StartedJobs { get; } = [];

    public ConcurrentDictionary<int, DateTimeOffset> CurrentlyProcessingWorkers { get; } = [];
}