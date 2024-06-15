using MediatR;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.Events.Events;

public class SubmissionAddedEvent : INotification
{
    public required Guid SubmissionId { get; init; }
    public required EPlatform Platform { get; init; }
    public required EEntityType EntityType { get; init; }
    public required bool AutoSubmit { get; init; }
}