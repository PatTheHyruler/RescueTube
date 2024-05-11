using RescueTube.Domain.Base;

namespace RescueTube.Domain.Entities;

public class CommentStatisticSnapshot : BaseIdDbEntity
{
    public long? LikeCount { get; set; }
    public long? DislikeCount { get; set; }
    public long? ReplyCount { get; set; }
    public bool? IsFavorited { get; set; }

    public DateTime ValidAt { get; set; }

    public Guid CommentId { get; set; }
    public Comment? Comment { get; set; }
}