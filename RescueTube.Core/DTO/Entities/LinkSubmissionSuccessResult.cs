using System.Diagnostics.CodeAnalysis;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.DTO.Entities;

public class LinkSubmissionSuccessResult
{
    public Guid SubmissionId { get; set; }
    public EEntityType Type { get; set; }
    public Guid? EntityId { get; set; }
    public EPlatform Platform { get; set; }
    public required string IdOnPlatform { get; set; } 
    public bool AlreadyAdded { get; set; }

    public LinkSubmissionSuccessResult()
    {
    }

    [SetsRequiredMembers]
    public LinkSubmissionSuccessResult(Submission submission, bool alreadyAdded)
    {
        SubmissionId = submission.Id;
        Type = submission.EntityType;
        EntityId = submission.EntityType switch
        {
            EEntityType.Video => submission.VideoId,
            EEntityType.Playlist => throw new NotImplementedException(),
            EEntityType.Author => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException(),
        };
        Platform = submission.Platform;
        IdOnPlatform = submission.IdOnPlatform;
        AlreadyAdded = alreadyAdded;
    }
}