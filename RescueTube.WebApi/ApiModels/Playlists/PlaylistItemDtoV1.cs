namespace WebApp.ApiModels.Playlists;

public record PlaylistItemDtoV1
{
    public required Guid Id { get; init; }
    public required VideoSimpleDtoV1 Video { get; init; }
    public uint Position { get; init; }
    public DateTimeOffset? AddedAt { get; init; }
    public DateTimeOffset? RemovedAt { get; init; }
}