using System.Diagnostics.CodeAnalysis;
using RescueTube.Domain.Base;
using RescueTube.Domain.Entities.Identity;
using RescueTube.Domain.Enums;

namespace RescueTube.Domain.Entities;

public class Submission : BaseIdDbEntity
{
    public required EPlatform Platform { get; set; }
    public required string IdOnPlatform { get; set; }
    public string? IdType { get; set; }
    public required EEntityType EntityType { get; set; }

    public string? Url { get; set; }

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

    public Guid? AuthorId { get; set; }
    public Author? Author { get; set; }

    public Submission()
    {
    }

    public Submission(Guid submitterId, bool autoSubmit)
    {
        AddedById = submitterId;
        AddedAt = DateTimeOffset.UtcNow;

        ApprovedById = autoSubmit ? submitterId : null;
        ApprovedAt = autoSubmit ? DateTimeOffset.UtcNow : null;
    }

    [SetsRequiredMembers]
    public Submission(RecognizedPlatformUrl recognizedPlatformUrl, Guid submitterId, bool autoSubmit)
        : this(submitterId: submitterId, autoSubmit: autoSubmit)
    {
        IdOnPlatform = recognizedPlatformUrl.IdOnPlatform;
        Platform = recognizedPlatformUrl.Platform;
        EntityType = recognizedPlatformUrl.EntityType;
        Url = recognizedPlatformUrl.Url;
        IdType = recognizedPlatformUrl.IdType;
    }
}