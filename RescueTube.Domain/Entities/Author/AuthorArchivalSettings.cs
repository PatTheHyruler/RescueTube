using RescueTube.Domain.Base;

namespace RescueTube.Domain.Entities;

public class AuthorArchivalSettings : BaseIdDbEntity
{
    public bool Active { get; set; }
    public bool ArchiveClips { get; set; } = true;
    public bool ArchivePlaylists { get; set; }
    public bool ArchiveVideos { get; set; } = true;
    public List<DateTimeRange>? ArchiveVideosFromDateTimeRanges { get; set; }

    public Author? Author { get; set; }

    public static AuthorArchivalSettings ArchivedDefault()
    {
        return new AuthorArchivalSettings
        {
            Active = true,
            ArchiveClips = true,
            ArchivePlaylists = false,
            ArchiveVideos = true,
            ArchiveVideosFromDateTimeRanges = null,
        };
    }
}