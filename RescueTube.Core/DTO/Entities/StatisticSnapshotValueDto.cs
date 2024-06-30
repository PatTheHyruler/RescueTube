namespace RescueTube.Core.DTO.Entities;

public class StatisticSnapshotValueDto<TValue>
{
    public required TValue Value { get; set; }
    public DateTimeOffset ValidAt { get; set; }
}