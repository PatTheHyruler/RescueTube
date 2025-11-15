using RescueTube.Domain.Base;

namespace RescueTube.Domain.Entities;

public sealed class JobSettings : BaseIdDbEntity
{
    public required string JobId { get; init; }

    public required bool IsEnabled { get; set; }
    public required string Cron { get; set; }

    public DataFetchJobSettings? DataFetchJobSettings { get; init; }
}

public sealed class DataFetchJobSettings
{
    public required TimeSpan SuccessCutoffOffset { get; init; }
    public required TimeSpan FailureCutoffOffset { get; init; }

    public static readonly DataFetchJobSettings Default = new()
    {
        SuccessCutoffOffset = TimeSpan.MaxValue,
        FailureCutoffOffset = TimeSpan.FromDays(10),
    };
}