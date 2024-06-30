namespace RescueTube.Core.DTO.Entities;

public class PlaylistItemDto<TVideo>
{
    public required Guid Id { get; set; }
    public required TVideo Video { get; set; }
    public uint Position { get; set; }
    public DateTimeOffset? AddedAt { get; set; }
    public DateTimeOffset? RemovedAt { get; set; }
}