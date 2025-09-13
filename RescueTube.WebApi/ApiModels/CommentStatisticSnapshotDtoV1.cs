namespace WebApp.ApiModels;

public class CommentStatisticSnapshotDtoV1
{
    public long? LikeCount { get; set; }
    public long? DislikeCount { get; set; }
    public long? ReplyCount { get; set; }
    public bool? IsFavorited { get; set; }

    public DateTimeOffset ValidAt { get; set; }
}