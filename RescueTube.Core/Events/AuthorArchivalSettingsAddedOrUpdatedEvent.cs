using MediatR;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.Events;

public class AuthorArchivalSettingsAddedOrUpdatedEvent : INotification
{
    public required Guid AuthorId { get; set; }
    public required Guid ArchivalSettingsId { get; set; }
    public required EPlatform Platform { get; set; }
}