using RescueTube.Domain.Base;
using RescueTube.Domain.Contracts;
using RescueTube.Domain.Entities.Localization;
using RescueTube.Domain.Enums;

namespace RescueTube.Domain.Entities;

public class Video : BaseIdDbEntity, IMainArchiveEntity
{
    public Guid? TitleId { get; set; }
    public TextTranslationKey? Title { get; set; }

    public Guid? DescriptionId { get; set; }
    public TextTranslationKey? Description { get; set; }
    public EVideoType? Type { get; set; }

    public string? DefaultLanguage { get; set; }
    public string? DefaultAudioLanguage { get; set; }

    public TimeSpan? Duration { get; set; }

    public List<VideoStatisticSnapshot>? VideoStatisticSnapshots { get; set; }

    public List<Caption>? Captions { get; set; }
    public List<VideoImage>? VideoImages { get; set; }

    public List<VideoTag>? VideoTags { get; set; }

    public List<VideoFile>? VideoFiles { get; set; }

    public string? InfoJsonPath { get; set; }
    public string? InfoJson { get; set; }
    // TODO: Replace LastCommentsFetch usages with DataFetches queries?
    // Currently keeping this for the sake of significantly simpler queries.
    public DateTimeOffset? LastCommentsFetch { get; set; }

    public ELiveStatus LiveStatus { get; set; } = ELiveStatus.None;
    public string? StreamId { get; set; }
    public DateTimeOffset? LiveStreamStartedAt { get; set; }
    public DateTimeOffset? LiveStreamEndedAt { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public DateTimeOffset? RecordedAt { get; set; }

    public EPlatform Platform { get; set; }
    public required string IdOnPlatform { get; set; }

    public EPrivacyStatus? PrivacyStatusOnPlatform { get; set; }
    public EPrivacyStatus PrivacyStatus { get; set; }

    public DateTimeOffset AddedToArchiveAt { get; set; }

    public ICollection<DataFetch>? DataFetches { get; set; }

    public ICollection<VideoAuthor>? VideoAuthors { get; set; }
    public ICollection<VideoCategory>? VideoCategories { get; set; }
    public ICollection<StatusChangeEvent>? StatusChangeEvents { get; set; }
    public ICollection<EntityAccessPermission>? EntityAccessPermissions { get; set; }
    public ICollection<Comment>? Comments { get; set; }

    public ICollection<PlaylistItem>? PlaylistItems { get; set; }
}