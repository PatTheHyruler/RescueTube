namespace WebApp.ApiModels.Playlists;

public record PlaylistSearchResponseDtoV1
{
    public required PaginationResultDtoV1 PaginationResult { get; set; }
    public required IEnumerable<PlaylistSimpleDtoV1> Playlists { get; set; }
}
