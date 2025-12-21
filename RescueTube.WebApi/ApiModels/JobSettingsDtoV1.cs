namespace RescueTube.WebApi.ApiModels;

public record JobSettingsDtoV1 : SimpleJobSettingsDtoV1
{
    public required bool IsArchivalJob { get; init; }

    public required SimpleJobSettingsDtoV1 DefaultSettings { get; init; }
}

public record SimpleJobSettingsDtoV1
{
    public required string JobId { get; init; }

    public required bool IsDefault { get; init; }

    public required bool IsEnabled { get; init; }
    public required string Cron { get; init; }

    public required DataFetchJobSettingsDtoV1? DataFetchJobSettings { get; init; }
}

public record DataFetchJobSettingsDtoV1 {
    public required TimeSpan SuccessCutoffOffset { get; init; }
    public required TimeSpan FailureCutoffOffset { get; init; }
}