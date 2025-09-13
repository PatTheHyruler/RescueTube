namespace RescueTube.WebApi.ApiModels;

public class AuthorArchivalSettingsDtoV1
{
    public required Guid Id { get; init; }
    public required Guid AuthorId { get; init; }

    public required bool IsEnabledForArchival { get; init; }
    public required bool ArchiveClips { get; init; }
    public required bool ArchivePlaylists { get; init; }
    public required bool ArchiveVideos { get; init; }
}