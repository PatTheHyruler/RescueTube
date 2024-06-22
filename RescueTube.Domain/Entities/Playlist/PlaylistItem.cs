using RescueTube.Domain.Base;

namespace RescueTube.Domain.Entities;

public class PlaylistItem : BaseIdDbEntity
{
    public Guid PlaylistId { get; set; }
    public Playlist? Playlist { get; set; }
    public Guid VideoId { get; set; }
    public Video? Video { get; set; }

    public uint Position { get; set; }

    public DateTimeOffset? AddedAt { get; set; }
    public DateTimeOffset? RemovedAt { get; set; }
    // TODO: AddedBy and RemovedBy?

    public List<PlaylistItemPositionHistory>? PositionHistories { get; set; }
}