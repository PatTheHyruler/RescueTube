using RescueTube.Domain.Enums;

namespace WebApp.ApiModels.Playlists;

public record PlaylistSimpleDtoV1
{
    public required Guid Id { get; init; }
    public required TextTranslationDtoV1[] Title { get; init; }
    public required TextTranslationDtoV1[] Description { get; init; }

    public required int VideosCount { get; init; }

    public required ImageDtoV1? Thumbnail { get; init; }
    public required string? UrlOnPlatform { get; init; }

    public required EPlatform Platform { get; init; }
    public required string IdOnPlatform { get; init; }

    public required DateTimeOffset AddedToArchiveAt { get; init; }
    public required DateTimeOffset? CreatedAt { get; init; }
    public required DateTimeOffset? UpdatedAt { get; init; }
}
