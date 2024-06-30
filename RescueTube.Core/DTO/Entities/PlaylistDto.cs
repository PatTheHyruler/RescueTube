using RescueTube.Domain.Contracts;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Entities.Localization;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.DTO.Entities;

public class PlaylistWithVideosDto<TVideo> : PlaylistDto
{
    public required List<PlaylistItemDto<TVideo>> Items { get; set; }
}

public class PlaylistDto : IMainArchiveEntity
{
    public required ICollection<TextTranslation> Title { get; set; }
    public required ICollection<TextTranslation> Description { get; set; }

    public Image? Thumbnail { get; set; }
    public string? UrlOnPlatform { get; set; }

    public required AuthorSimple? Creator { get; set; }

    public EPrivacyStatus? PrivacyStatusOnPlatform { get; set; }
    public required EPrivacyStatus PrivacyStatus { get; set; }

    public required CombinedStatisticSnapshotDto Statistics { get; set; }

    public required Guid Id { get; set; }
    public required EPlatform Platform { get; set; }
    public required string IdOnPlatform { get; set; }
    public required DateTimeOffset? LastFetchUnofficial { get; set; }
    public required DateTimeOffset? LastSuccessfulFetchUnofficial { get; set; }
    public required DateTimeOffset? LastFetchOfficial { get; set; }
    public required DateTimeOffset? LastSuccessfulFetchOfficial { get; set; }
    public required DateTimeOffset AddedToArchiveAt { get; set; }
    public required DateTimeOffset? CreatedAt { get; set; }
    public required DateTimeOffset? UpdatedAt { get; set; }
}