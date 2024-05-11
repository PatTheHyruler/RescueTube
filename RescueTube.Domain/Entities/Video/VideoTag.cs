using RescueTube.Domain.Base;

namespace RescueTube.Domain.Entities;

public class VideoTag : BaseIdDbEntity
{
    public required string Tag { get; set; }
    public required string NormalizedTag { get; set; }

    public DateTime? ValidSince { get; set; }
    public DateTime? ValidUntil { get; set; }

    public Guid VideoId { get; set; }
    public Video? Video { get; set; }
}