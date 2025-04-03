namespace RescueTube.Core.JobOrchestration;

public enum JobExecutionResult
{
    Succeeded,
    HasMoreToProcess,
    NothingToProcess,
    Errored,
}