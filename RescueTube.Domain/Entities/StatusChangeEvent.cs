using RescueTube.Domain.Base;
using RescueTube.Domain.Contracts;
using RescueTube.Domain.Enums;

namespace RescueTube.Domain.Entities;

public class StatusChangeEvent : BaseIdDbEntity
{
    public EPrivacyStatus? PreviousPrivacyStatus { get; set; }
    public EPrivacyStatus? NewPrivacyStatus { get; set; }
    public DateTimeOffset OccurredAt { get; set; }

    public Guid? AuthorId { get; set; }
    public Author? Author { get; set; }
    public Guid? VideoId { get; set; }
    public Video? Video { get; set; }

    public Guid? PlaylistId { get; set; }
    public Playlist? Playlist { get; set; }

    public StatusChangeEvent()
    {
    }

    private StatusChangeEvent(IPrivacyEntity entity, EPrivacyStatus? newPrivacyStatus, DateTimeOffset? occurredAt)
    {
        OccurredAt = occurredAt ?? DateTimeOffset.UtcNow;
        PreviousPrivacyStatus = entity.PrivacyStatus;
        NewPrivacyStatus = newPrivacyStatus;

        entity.PrivacyStatusOnPlatform = newPrivacyStatus;
    }

    public StatusChangeEvent(Video video, EPrivacyStatus? newPrivacyStatus,
        DateTimeOffset? occurredAt = null) :
        this(video as IPrivacyEntity, newPrivacyStatus, occurredAt)
    {
        VideoId = video.Id;
    }

    public StatusChangeEvent(Author author, EPrivacyStatus? newPrivacyStatus,
        DateTimeOffset? occurredAt = null) :
        this(author as IPrivacyEntity, newPrivacyStatus, occurredAt)
    {
        AuthorId = author.Id;
    }

    public StatusChangeEvent(Playlist playlist, EPrivacyStatus? newPrivacyStatus,
        DateTimeOffset? occurredAt = null) :
        this(playlist as IPrivacyEntity, newPrivacyStatus, occurredAt)
    {
        PlaylistId = playlist.Id;
    }
}