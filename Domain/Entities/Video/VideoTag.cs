using Domain.Base;

namespace Domain.Entities;

public class VideoTag : BaseIdDbEntity
{
    public string Tag { get; set; } = default!;
    public string NormalizedTag { get; set; } = default!;

    public DateTime? ValidSince { get; set; }
    public DateTime? ValidUntil { get; set; }

    public Guid VideoId { get; set; }
    public Video? Video { get; set; }
}