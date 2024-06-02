using RescueTube.Domain.Base;

namespace RescueTube.Domain.Entities;

public class VideoTag : BaseIdDbEntity
{
    public required string Tag { get; set; }
    public required string NormalizedTag { get; set; }

    public DateTimeOffset? ValidSince { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }

    public Guid VideoId { get; set; }
    public Video? Video { get; set; }
}