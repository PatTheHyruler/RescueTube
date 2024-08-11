using MediatR;

namespace RescueTube.Core.Mediator;

public class AddFailedDataFetchRequest : IRequest
{
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
    public required string Type { get; set; }
    public required string Source { get; set; }
    public required bool ShouldAffectValidity { get; set; }

    public Guid? VideoId { get; set; }
    public Guid? AuthorId { get; set; }
    public Guid? CommentId { get; set; }
    public Guid? PlaylistId { get; set; }
}