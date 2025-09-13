namespace RescueTube.WebApi.ApiModels.Playlists;

public record PlaylistSearchDtoV1 : IPaginationQueryOptionalDtoV1
{
    public PlaylistSearchFilterDtoV1? Filter { get; init; }

    public int? Page { get; init; }
    public int? Limit { get; init; }
    public int DefaultPage { get; init; } = 0;
    public int DefaultLimit { get; init; } = 50;
}