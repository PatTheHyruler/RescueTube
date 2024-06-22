using System.Diagnostics.CodeAnalysis;
using RescueTube.Domain.Base;
using RescueTube.Domain.Entities.Identity;
using RescueTube.Domain.Enums;

namespace RescueTube.Domain.Entities;

public class Submission : BaseIdDbEntity
{
    public EPlatform Platform { get; set; }
    public required string IdOnPlatform { get; set; }
    public EEntityType EntityType { get; set; }

    public Guid AddedById { get; set; }
    public User? AddedBy { get; set; }
    public DateTimeOffset AddedAt { get; set; }

    public Guid? ApprovedById { get; set; }
    public User? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public bool GrantAccess { get; set; } = true;

    public DateTimeOffset? CompletedAt { get; set; }

    public Guid? VideoId { get; set; }
    public Video? Video { get; set; }

    public Guid? PlaylistId { get; set; }
    public Playlist? Playlist { get; set; }

    public Submission()
    {
    }

    [SetsRequiredMembers]
    public Submission(Video video, Guid submitterId, bool autoSubmit) :
        this(video.IdOnPlatform, video.Platform, EEntityType.Video, submitterId, autoSubmit)
    {
        VideoId = video.Id;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    [SetsRequiredMembers]
    public Submission(string idOnPlatform, EPlatform platform, EEntityType entityType, Guid submitterId,
        bool autoSubmit)
    {
        Platform = platform;
        IdOnPlatform = idOnPlatform;
        EntityType = entityType;

        AddedById = submitterId;
        AddedAt = DateTimeOffset.UtcNow;

        ApprovedById = autoSubmit ? submitterId : null;
        ApprovedAt = autoSubmit ? DateTimeOffset.UtcNow : null;
    }
}