using RescueTube.Core.SourceGeneration;

namespace WebApp.ApiModels;

[Partial<VideoArchivalSettingsDtoV1>]
// ReSharper disable once ClassNeverInstantiated.Global
public partial record VideoArchivalSettingsPartialDtoV1;

public record VideoArchivalSettingsBulkUpdateDtoV1
{
    public required VideoSearchFilterDtoV1? Filter { get; init; }
    public bool SelectAll { get; init; }
    public Guid[]? VideoIds { get; init; }

    public required VideoArchivalSettingsPartialDtoV1 Settings { get; init; }
}
