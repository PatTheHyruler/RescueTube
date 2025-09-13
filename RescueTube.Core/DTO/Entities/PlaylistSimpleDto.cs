using RescueTube.Domain.Entities;
using RescueTube.Domain.Entities.Localization;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.DTO.Entities;

public class PlaylistSimpleDto : IPlaylistDto
{
    public required ICollection<TextTranslation> Title { get; init; }
    public required ICollection<TextTranslation> Description { get; init; }

    public required int VideosCount { get; init; }

    public required Image? Thumbnail { get; init; }
    public string? UrlOnPlatform { get; set; }

    public required AuthorSimple? Creator { get; init; }

    public required EPrivacyStatus? PrivacyStatusOnPlatform { get; init; }

    public required Guid Id { get; init; }
    public required EPlatform Platform { get; init; }
    public required string IdOnPlatform { get; init; }
    public required DateTimeOffset AddedToArchiveAt { get; init; }
    public required DateTimeOffset? CreatedAt { get; init; }
    public required DateTimeOffset? UpdatedAt { get; init; }
}