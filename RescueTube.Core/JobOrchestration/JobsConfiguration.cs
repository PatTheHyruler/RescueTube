namespace RescueTube.Core.JobOrchestration;

public class JobsConfiguration
{
    private readonly HashSet<JobDefinition> _registeredJobs = [];

    public void RegisterJobs(params ReadOnlySpan<JobDefinition> jobDefinitions)
    {
        foreach (var jobDefinition in jobDefinitions)
        {
            // TODO: Validate definitions?
            _registeredJobs.Add(jobDefinition);
        }
    }

    public IReadOnlyCollection<JobDefinition> RegisteredJobs => _registeredJobs;
}