using MediatR;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Events;

public record SettingChangingEvent(Setting Setting, SettingChangingEvent.ChangeType Type) : INotification
{
    public enum ChangeType
    {
        Added,
        Updated,
        Removed,
    }
}