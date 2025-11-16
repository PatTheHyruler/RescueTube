namespace RescueTube.WebApi.ApiModels;

public record JobSettingsUpdateDtoV1
{
    public required string JobId { get; init; }

    public required bool IsEnabled { get; init; }
    public required string Cron { get; init; }

    public required DataFetchJobSettingsDtoV1? DataFetchJobSettings { get; init; }
}