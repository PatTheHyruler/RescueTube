namespace RescueTube.Core.JobOrchestration;

public interface IJob
{
    public Task<JobExecutionResult> RunAsync(CancellationToken ct);
}