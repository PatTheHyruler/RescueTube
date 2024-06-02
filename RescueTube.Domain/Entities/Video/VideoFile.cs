using RescueTube.Domain.Base;

namespace RescueTube.Domain.Entities;

public class VideoFile : BaseIdDbEntity
{
    public string? Key { get; set; }
    public required string FilePath { get; set; }
    public string? Etag { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? BitrateBps { get; set; }

    public DateTimeOffset? ValidSince { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }
    public DateTimeOffset? LastFetched { get; set; }

    public Guid VideoId { get; set; }
    public Video? Video { get; set; }
}