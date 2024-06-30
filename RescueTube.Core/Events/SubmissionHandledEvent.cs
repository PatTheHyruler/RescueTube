using MediatR;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.Events;

public class SubmissionHandledEvent : INotification
{
    public required Guid SubmissionId { get; set; }
    public required EPlatform Platform { get; set; }
    public required EEntityType EntityType { get; set; }
}