namespace RescueTube.Core.JobOrchestration;

public record JobInvocationInfo
{
    public JobInvocationInfo(DateTimeOffset startedAt)
    {
        StartedAt = startedAt;
    }

    public void MarkFinished(DateTimeOffset finishedAt, JobExecutionResult result)
    {
        if (FinishedAt is not null)
        {
            throw new InvalidOperationException("Can't mark already finished invocation as finished");
        }
        ArgumentOutOfRangeException.ThrowIfLessThan(finishedAt, StartedAt);
        FinishedAt = finishedAt;
        Result = result;
    }

    public DateTimeOffset StartedAt { get; }
    public DateTimeOffset? FinishedAt { get; private set; }

    public JobExecutionResult? Result { get; private set; }

    public bool IsRunning => FinishedAt is null;
}