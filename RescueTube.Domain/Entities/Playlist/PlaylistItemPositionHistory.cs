using RescueTube.Domain.Base;

namespace RescueTube.Domain.Entities;

public class PlaylistItemPositionHistory : BaseIdDbEntity
{
    public Guid? PlaylistItemId { get; set; }
    public PlaylistItem? PlaylistItem { get; set; }

    public uint Position { get; set; }

    public DateTimeOffset? ValidSince { get; set; }
    public DateTimeOffset ValidUntil { get; set; }
}