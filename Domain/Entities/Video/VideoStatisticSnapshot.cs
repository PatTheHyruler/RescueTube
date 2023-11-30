using Domain.Base;

namespace Domain.Entities;

public class VideoStatisticSnapshot : BaseIdDbEntity
{
    public long? ViewCount { get; set; }
    public long? LikeCount { get; set; }
    public long? DislikeCount { get; set; }
    public long? CommentCount { get; set; }

    public DateTime ValidAt { get; set; }

    public Guid VideoId { get; set; }
    public Video? Video { get; set; }
}