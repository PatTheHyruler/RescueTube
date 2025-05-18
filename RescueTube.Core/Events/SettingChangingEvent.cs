using MediatR;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Events;

public record SettingChangingEvent(Setting Setting, SettingChangingEvent.ChangeType Type) : INotification
{
    public static SettingChangingEvent Added(Setting setting)
    {
        return new(Setting: setting, Type: ChangeType.Added);
    }

    public static SettingChangingEvent Updated(Setting setting)
    {
        return new(Setting: setting, Type: ChangeType.Updated);
    }

    public static SettingChangingEvent Removed(Setting setting)
    {
        return new(Setting: setting, Type: ChangeType.Removed);
    }

    public enum ChangeType
    {
        Added,
        Updated,
        Removed,
    }
}