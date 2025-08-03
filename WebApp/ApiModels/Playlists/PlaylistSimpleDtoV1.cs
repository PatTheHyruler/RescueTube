namespace WebApp.ApiModels.Playlists;

public record PlaylistSimpleDtoV1
{
    public required Guid Id { get; init; }
    public required ImageDtoV1? Thumbnail { get; init; }
}
