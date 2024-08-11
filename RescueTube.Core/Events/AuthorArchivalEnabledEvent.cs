using MediatR;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.Events;

public class AuthorArchivalEnabledEvent : INotification
{
    public required Guid AuthorId { get; set; }
    public required EPlatform Platform { get; set; }

    public required AuthorArchivalSettings AuthorArchivalSettings { get; set; }
}