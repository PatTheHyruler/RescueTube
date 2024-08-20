using RescueTube.Domain.Base;
using RescueTube.Domain.Contracts;
using RescueTube.Domain.Enums;

namespace RescueTube.Domain.Entities;

public class VideoImage : BaseIdDbEntity, IEntityImage
{
    public EImageType ImageType { get; set; }

    public DateTimeOffset? ValidSince { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }
    public DateTimeOffset? LastFetched { get; set; }

    public int? Preference { get; set; }

    public Guid VideoId { get; set; }
    public Video? Video { get; set; }

    public Guid ImageId { get; set; }
    public Image? Image { get; set; }
}