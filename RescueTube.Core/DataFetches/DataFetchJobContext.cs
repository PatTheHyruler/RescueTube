using System.Collections.Concurrent;

namespace RescueTube.Core.DataFetches;

public class DataFetchJobContext
{
    public ConcurrentDictionary<
        DataFetchDefinition,
        ConcurrentDictionary<Guid, JobInvocationInfo>
    > StartedJobs { get; } = [];

    public ConcurrentDictionary<int, DateTimeOffset> CurrentlyProcessingWorkers { get; } = [];
}