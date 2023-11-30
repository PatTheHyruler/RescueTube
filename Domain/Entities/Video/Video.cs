using Domain.Base;
using Domain.Contracts;
using Domain.Entities.Localization;
using Domain.Enums;

namespace Domain.Entities;

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
    public DateTime? LastCommentsFetch { get; set; }

    public bool? IsLiveStreamRecording { get; set; }
    public string? StreamId { get; set; }
    public DateTime? LiveStreamStartedAt { get; set; }
    public DateTime? LiveStreamEndedAt { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? RecordedAt { get; set; }
    
    public EPlatform Platform { get; set; }
    public string IdOnPlatform { get; set; } = default!;

    public EPrivacyStatus? PrivacyStatusOnPlatform { get; set; }
    public bool IsAvailable { get; set; }
    public EPrivacyStatus PrivacyStatus { get; set; }

    public DateTime? LastFetchUnofficial { get; set; }
    public DateTime? LastSuccessfulFetchUnofficial { get; set; }
    public DateTime? LastFetchOfficial { get; set; }
    public DateTime? LastSuccessfulFetchOfficial { get; set; }
    public DateTime AddedToArchiveAt { get; set; }

    public ICollection<VideoAuthor>? VideoAuthors { get; set; }
    public ICollection<VideoCategory>? VideoCategories { get; set; }
    public ICollection<StatusChangeEvent>? StatusChangeEvents { get; set; }
    public ICollection<EntityAccessPermission>? EntityAccessPermissions { get; set; }
    public ICollection<Comment>? Comments { get; set; }
}