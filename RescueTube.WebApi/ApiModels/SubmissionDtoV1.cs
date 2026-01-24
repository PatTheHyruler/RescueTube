using RescueTube.Domain.Enums;
using RescueTube.WebApi.ApiModels.Auth;

namespace RescueTube.WebApi.ApiModels;

public record SubmissionDtoV1
{
    public required Guid Id { get; init; }

    public required EPlatform Platform { get; init; }
    public required string IdOnPlatform { get; init; }
    public required string? IdType { get; init; }
    public required EEntityType EntityType { get; init; }

    public required string? Url { get; init; }

    public required UserSimpleDtoV1 AddedBy { get; init; }
    public required DateTimeOffset AddedAt { get; init; }

    public required UserSimpleDtoV1? ApprovedBy { get; init; }
    public required DateTimeOffset? ApprovedAt { get; init; }
    public required bool GrantAccess { get; init; }

    public required DateTimeOffset? CompletedAt { get; init; }

    public required Guid? VideoId { get; init; }
    public required Guid? PlaylistId { get; init; }
    public required Guid? AuthorId { get; init; }

    public required SubmissionHandlingFailureDtoV1[] Failures { get; init; }
}

public record SubmissionHandlingFailureDtoV1
{
    public required Guid Id { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
    public required string Reason { get; init; }
}