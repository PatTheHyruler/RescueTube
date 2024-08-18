using RescueTube.Domain.Base;
using RescueTube.Domain.Enums;

namespace RescueTube.Domain.Entities;

public class AuthorImage : BaseIdDbEntity
{
    public EImageType? ImageType { get; set; }
    public short ImageTypeDetectionAttempts { get; set; }

    public DateTimeOffset? ValidSince { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }
    public DateTimeOffset? LastFetched { get; set; }

    public Guid AuthorId { get; set; }
    public Author? Author { get; set; }
    public Guid ImageId { get; set; }
    public Image? Image { get; set; }
}