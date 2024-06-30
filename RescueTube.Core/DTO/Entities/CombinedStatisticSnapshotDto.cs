namespace RescueTube.Core.DTO.Entities;

public class CombinedStatisticSnapshotDto
{
    public StatisticSnapshotValueDto<long>? ViewCount { get; set; }
    public StatisticSnapshotValueDto<long>? LikeCount { get; set; }
    public StatisticSnapshotValueDto<long>? DislikeCount { get; set; }
    public StatisticSnapshotValueDto<long>? CommentCount { get; set; }
}