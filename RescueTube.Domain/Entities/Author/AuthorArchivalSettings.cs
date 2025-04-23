using RescueTube.Domain.Base;

namespace RescueTube.Domain.Entities;

public class AuthorArchivalSettings : BaseIdDbEntity
{
    public bool IsEnabledForArchival { get; set; }
    public bool ArchiveClips { get; set; } = true;
    public bool ArchivePlaylists { get; set; }
    public bool ArchiveVideos { get; set; } = true;

    public Author? Author { get; set; }

    public static AuthorArchivalSettings CreateDefaultArchivedAuthorSettings()
    {
        return new AuthorArchivalSettings
        {
            IsEnabledForArchival = true,
            ArchiveClips = true,
            ArchivePlaylists = false,
            ArchiveVideos = true,
        };
    }
}