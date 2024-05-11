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

    public string? DefaultLanguage { get; set; }
    public string? DefaultAudioLanguage { get; set; }

    public TimeSpan? Duration { get; set; }
    
    public ICollection<VideoStatisticSnapshot>? VideoStatisticSnapshots { get; set; }

    public ICollection<Caption>? Captions { get; set; }
    public ICollection<VideoImage>? VideoImages { get; set; }

    public ICollection<VideoTag>? VideoTags { get; set; }

    public ICollection<VideoFile>? VideoFiles { get; set; }

    public int FailedDownloadAttempts { get; set; }
    public int FailedAuthorFetches { get; set; }
    public string? InfoJsonPath { get; set; }
    public string? InfoJson { get; set; }
    public DateTimeOffset? LastCommentsFetch { get; set; }

    public bool? IsLiveStreamRecording { get; set; }
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
    public bool IsAvailable { get; set; }
    public EPrivacyStatus PrivacyStatus { get; set; }

    public DateTimeOffset? LastFetchUnofficial { get; set; }
    public DateTimeOffset? LastSuccessfulFetchUnofficial { get; set; }
    public DateTimeOffset? LastFetchOfficial { get; set; }
    public DateTimeOffset? LastSuccessfulFetchOfficial { get; set; }
    public DateTimeOffset AddedToArchiveAt { get; set; }

    public ICollection<VideoAuthor>? VideoAuthors { get; set; }
    public ICollection<VideoCategory>? VideoCategories { get; set; }
    public ICollection<StatusChangeEvent>? StatusChangeEvents { get; set; }
    public ICollection<EntityAccessPermission>? EntityAccessPermissions { get; set; }
    public ICollection<Comment>? Comments { get; set; }
}