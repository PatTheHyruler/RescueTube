using RescueTube.Domain.Base;
using RescueTube.Domain.Enums;

namespace RescueTube.Domain.Entities;

public class PlaylistImage : BaseIdDbEntity
{
    public EImageType ImageType { get; set; }

    public DateTimeOffset? ValidSince { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }
    public DateTimeOffset? LastFetched { get; set; }

    public Guid PlaylistId { get; set; }
    public Playlist? Playlist { get; set; }

    public Guid ImageId { get; set; }
    public Image? Image { get; set; }
}