namespace RescueTube.Core.DTO.Entities;

public class CommentStatisticSnapshotDto
{
    public long? LikeCount { get; set; }
    public long? DislikeCount { get; set; }
    public long? ReplyCount { get; set; }
    public bool? IsFavorited { get; set; }

    public DateTime ValidAt { get; set; }
}