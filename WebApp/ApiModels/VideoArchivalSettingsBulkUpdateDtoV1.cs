namespace WebApp.ApiModels;

public record VideoArchivalSettingsBulkUpdateDtoV1
{
    public required VideoSearchFilterDtoV1? Filter { get; init; }
    public Guid[]? VideoIds { get; init; }

    public required VideoArchivalSettingsDtoV1 Settings { get; init; }
}
