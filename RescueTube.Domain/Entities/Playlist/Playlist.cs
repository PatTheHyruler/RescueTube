using RescueTube.Domain.Base;
using RescueTube.Domain.Contracts;
using RescueTube.Domain.Entities.Localization;
using RescueTube.Domain.Enums;

namespace RescueTube.Domain.Entities;

public class Playlist : BaseIdDbEntity, IMainArchiveEntity
{
    public Guid? TitleId { get; set; }
    public TextTranslationKey? Title { get; set; }

    public Guid? DescriptionId { get; set; }
    public TextTranslationKey? Description { get; set; }

    public EPlatform Platform { get; set; }
    public required string IdOnPlatform { get; set; }
    public EPrivacyStatus? PrivacyStatusOnPlatform { get; set; }
    public EPrivacyStatus PrivacyStatus { get; set; }
    public DateTimeOffset? LastFetchUnofficial { get; set; }
    public DateTimeOffset? LastSuccessfulFetchUnofficial { get; set; }
    public DateTimeOffset? LastFetchOfficial { get; set; }
    public DateTimeOffset? LastSuccessfulFetchOfficial { get; set; }
    public DateTimeOffset AddedToArchiveAt { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<PlaylistItem>? PlaylistItems { get; set; }
    public ICollection<PlaylistImage>? PlaylistImages { get; set; }

    public Guid CreatorId { get; set; }
    public Author? Creator { get; set; }

    public ICollection<PlaylistStatisticSnapshot>? PlaylistStatisticSnapshots { get; set; }
    public ICollection<StatusChangeEvent>? StatusChangeEvents { get; set; }
}