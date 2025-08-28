using RescueTube.Core.Utils;

namespace RescueTube.Core.DataFetches;

public class DataFetchJobsConfiguration
{
    private readonly HashSet<DataFetchJobRegistration> _registeredJobs = [];

    public IReadOnlyCollection<DataFetchJobRegistration> RegisteredJobs => _registeredJobs;

    public DataFetchJobsConfiguration RegisterJob<T>() where T : IEntityDataFetchJobWithDefinition
    {
        _registeredJobs.Add(new DataFetchJobRegistration(typeof(T), T.JobDefinition));
        return this;
    }

    public sealed record DataFetchJobRegistration(Type JobType, DataFetchJobDefinition Definition)
    {
        public string JobName => JobType.FullName.AssertNotNull();
    }
}