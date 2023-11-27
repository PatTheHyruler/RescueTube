using Base.Domain;

namespace Domain.Entities;

public class CommentStatisticSnapshot : AbstractIdDatabaseEntity
{
    public long? LikeCount { get; set; }
    public long? DislikeCount { get; set; }
    public long? ReplyCount { get; set; }
    public bool? IsFavorited { get; set; }

    public DateTime ValidAt { get; set; }

    public Guid CommentId { get; set; }
    public Comment? Comment { get; set; }
}