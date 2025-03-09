using RescueTube.Domain.Enums;

namespace RescueTube.Domain;

public readonly record struct DataFetchJobDefinition
{
    public DataFetchJobDefinition(
        DataFetchDefinition dataFetchDefinition,
        TimeSpan successCutoffOffset,
        TimeSpan failureCutoffOffset
    )
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(successCutoffOffset, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfLessThan(failureCutoffOffset, TimeSpan.Zero);
        if (dataFetchDefinition == default)
        {
            throw new ArgumentOutOfRangeException(nameof(dataFetchDefinition), dataFetchDefinition, "Data fetch definition cannot be empty.");
        }

        DataFetchDefinition = dataFetchDefinition;
        SuccessCutoffOffset = successCutoffOffset;
        FailureCutoffOffset = failureCutoffOffset;
    }

    public DataFetchDefinition DataFetchDefinition { get; private init; }
    public TimeSpan SuccessCutoffOffset { get; private init; }
    public TimeSpan FailureCutoffOffset { get; private init; }
}