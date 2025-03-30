namespace RescueTube.Core.DataFetches;

public class DataFetchJobsConfiguration
{
    private readonly Dictionary<DataFetchDefinition, DataFetchJobDefinition> _registeredJobs = [];

    public void RegisterJobs(params ReadOnlySpan<DataFetchJobDefinition> jobDefinitions)
    {
        foreach (var jobDefinition in jobDefinitions)
        {
            // TODO: Validate definitions?
            _registeredJobs.Add(jobDefinition.DataFetchDefinition, jobDefinition);
        }
    }

    public IReadOnlyCollection<DataFetchJobDefinition> RegisteredJobs => _registeredJobs.Values;
}