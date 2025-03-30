namespace RescueTube.Core.DataFetches;

public record JobInvocationInfo
{
    public JobInvocationInfo(DateTimeOffset startedAt)
    {
        StartedAt = startedAt;
    }

    public void MarkFinished(DateTimeOffset finishedAt)
    {
        if (FinishedAt is not null)
        {
            throw new InvalidOperationException("Can't mark already finished invocation as finished");
        }
        ArgumentOutOfRangeException.ThrowIfLessThan(finishedAt, StartedAt);
        FinishedAt = finishedAt;
    }

    public DateTimeOffset StartedAt { get; }
    public DateTimeOffset? FinishedAt { get; private set; }

    public bool IsRunning => FinishedAt is null;
}