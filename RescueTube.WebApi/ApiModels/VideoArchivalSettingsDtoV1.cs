namespace WebApp.ApiModels;

public record VideoArchivalSettingsDtoV1
{
    public required bool ShouldRegularlyFetchVideoData { get; init; }
    public required int DownloadPriority { get; init; }
}