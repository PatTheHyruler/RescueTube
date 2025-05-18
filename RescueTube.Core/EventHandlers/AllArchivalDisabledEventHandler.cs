using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RescueTube.Core.Constants;
using RescueTube.Core.Events;
using RescueTube.Core.Jobs.Registration;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.EventHandlers;

public class AllArchivalDisabledEventHandler : INotificationHandler<SettingChangingEvent>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public AllArchivalDisabledEventHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task Handle(SettingChangingEvent notification, CancellationToken cancellationToken)
    {
        if (notification.Setting.Key != SettingDefinitions.DisableAllArchival.Key
            || notification.Setting is not Setting.Bool setting)
        {
            return;
        }

#pragma warning disable CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
        var newValue = (setting.Value, notification.Type) switch
#pragma warning restore CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
        {
            (true, SettingChangingEvent.ChangeType.Added or SettingChangingEvent.ChangeType.Updated) => true,
            (false, SettingChangingEvent.ChangeType.Added or SettingChangingEvent.ChangeType.Updated) => false,
            (_, SettingChangingEvent.ChangeType.Removed) => SettingDefinitions.DisableAllArchival.DefaultValue,
        };

        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var recurringJobsService = scope.ServiceProvider.GetRequiredService<RecurringJobsService>();

        if (newValue)
        {
            recurringJobsService.DeleteArchivalRecurringJobs();
        }
        else
        {
            recurringJobsService.CreateArchivalRecurringJobs();
        }
    }
}