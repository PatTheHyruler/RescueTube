using RescueTube.Domain.Base;
using RescueTube.Domain.Contracts;
using RescueTube.Domain.Enums;

namespace RescueTube.Domain.Entities;

public class StatusChangeEvent : BaseIdDbEntity
{
    public bool? PreviousAvailability { get; set; }
    public bool? NewAvailability { get; set; }
    public EPrivacyStatus? PreviousPrivacyStatus { get; set; }
    public EPrivacyStatus? NewPrivacyStatus { get; set; }
    public DateTimeOffset OccurredAt { get; set; }

    public Guid? AuthorId { get; set; }
    public Author? Author { get; set; }
    public Guid? VideoId { get; set; }
    public Video? Video { get; set; }

    public StatusChangeEvent()
    {
    }
    
    private StatusChangeEvent(IPrivacyEntity entity, EPrivacyStatus? newPrivacyStatus,
        bool? newAvailability, DateTimeOffset? occurredAt)
    {
        OccurredAt = occurredAt ?? DateTimeOffset.UtcNow;
        PreviousAvailability = entity.IsAvailable;
        NewAvailability = newAvailability;
        PreviousPrivacyStatus = entity.PrivacyStatus;
        NewPrivacyStatus = newPrivacyStatus;

        entity.PrivacyStatusOnPlatform = newPrivacyStatus;
        entity.IsAvailable = newAvailability ?? entity.IsAvailable;
    }

    public StatusChangeEvent(Video video, EPrivacyStatus? newPrivacyStatus, bool? newAvailability,
        DateTimeOffset? occurredAt = null) :
        this(video as IPrivacyEntity, newPrivacyStatus, newAvailability, occurredAt)
    {
        VideoId = video.Id;
    }
    
    public StatusChangeEvent(Author author, EPrivacyStatus? newPrivacyStatus, bool? newAvailability,
        DateTimeOffset? occurredAt = null) :
        this(author as IPrivacyEntity, newPrivacyStatus, newAvailability, occurredAt)
    {
        AuthorId = author.Id;
    }
}