namespace RescueTube.Core.DataFetches;

public record DataFetchJobDefinition
{
    public required DataFetchDefinition DataFetchDefinition { get; init; }

    public required TimeSpan SuccessCutoffOffset { get; init; }
    public required TimeSpan FailureCutoffOffset { get; init; }
}