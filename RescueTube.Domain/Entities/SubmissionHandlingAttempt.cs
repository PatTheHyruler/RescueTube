using RescueTube.Domain.Base;

namespace RescueTube.Domain.Entities;

public sealed class SubmissionHandlingFailure : BaseIdDbEntity
{
    public Guid SubmissionId { get; set; }
    public Submission? Submission { get; set; }

    public required DateTimeOffset OccurredAt { get; init; }
    public required string Reason { get; init; }
}