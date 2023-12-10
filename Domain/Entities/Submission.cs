using System.Diagnostics.CodeAnalysis;
using Domain.Base;
using Domain.Entities.Identity;
using Domain.Enums;

namespace Domain.Entities;

public class Submission : BaseIdDbEntity
{
    public EPlatform Platform { get; set; }
    public required string IdOnPlatform { get; set; }
    public EEntityType EntityType { get; set; }

    public Guid AddedById { get; set; }
    public User? AddedBy { get; set; }
    public DateTime AddedAt { get; set; }

    public Guid? ApprovedById { get; set; }
    public User? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public bool GrantAccess { get; set; } = true;

    public DateTime? CompletedAt { get; set; }

    public Guid? VideoId { get; set; }
    public Video? Video { get; set; }

    public Submission()
    {
    }

    [SetsRequiredMembers]
    public Submission(Video video, Guid submitterId, bool autoSubmit) :
        this(video.IdOnPlatform, video.Platform, EEntityType.Video, submitterId, autoSubmit)
    {
        VideoId = video.Id;
        CompletedAt = DateTime.UtcNow;
    }

    [SetsRequiredMembers]
    public Submission(string idOnPlatform, EPlatform platform, EEntityType entityType, Guid submitterId,
        bool autoSubmit)
    {
        Platform = platform;
        IdOnPlatform = idOnPlatform;
        EntityType = entityType;

        AddedById = submitterId;
        AddedAt = DateTime.UtcNow;

        ApprovedById = autoSubmit ? submitterId : null;
        ApprovedAt = autoSubmit ? DateTime.UtcNow : null;
    }
}