using Base.Domain;

namespace Domain.Entities;

public class VideoFile : AbstractIdDatabaseEntity
{
    public string? Key { get; set; }
    public string FilePath { get; set; } = default!;
    public string? Etag { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? BitrateBps { get; set; }

    public DateTime? ValidSince { get; set; }
    public DateTime? ValidUntil { get; set; }
    public DateTime? LastFetched { get; set; }

    public Guid VideoId { get; set; }
    public Video? Video { get; set; }
}