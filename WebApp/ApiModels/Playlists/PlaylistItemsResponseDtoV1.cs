namespace WebApp.ApiModels.Playlists;

public record PlaylistItemsResponseDtoV1
{
    public required PaginationResultDtoV1 PaginationResult { get; init; }
    public required IEnumerable<PlaylistItemDtoV1> PlaylistItems { get; init; }
}