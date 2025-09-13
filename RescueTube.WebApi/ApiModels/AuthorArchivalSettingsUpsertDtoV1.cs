namespace RescueTube.WebApi.ApiModels;

public class AuthorArchivalSettingsUpsertDtoV1
{
    public required bool IsEnabledForArchival { get; init; }
    public required bool ArchiveClips { get; init; }
    public required bool ArchivePlaylists { get; init; }
    public required bool ArchiveVideos { get; init; }
}