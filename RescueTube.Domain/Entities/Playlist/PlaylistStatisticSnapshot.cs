using RescueTube.Domain.Base;

namespace RescueTube.Domain.Entities;

public class PlaylistStatisticSnapshot : BaseIdDbEntity
{
    public long? ViewCount { get; set; }
    public long? LikeCount { get; set; }
    public long? DislikeCount { get; set; }
    public long? CommentCount { get; set; }

    public DateTimeOffset ValidAt { get; set; }

    public Guid PlaylistId { get; set; }
    public Playlist? Playlist { get; set; }
}