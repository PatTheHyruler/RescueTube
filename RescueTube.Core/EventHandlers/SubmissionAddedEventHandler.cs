using MediatR;
using RescueTube.Core.Events;
using RescueTube.Core.Jobs;
using RescueTube.Core.Services;

namespace RescueTube.Core.EventHandlers;

public class SubmissionAddedEventHandler(IRecurringJobsService recurringJobsService) : INotificationHandler<SubmissionAddedEvent>
{
    public Task Handle(SubmissionAddedEvent notification, CancellationToken cancellationToken)
    {
        if (notification is { AutoSubmit: true })
        {
            recurringJobsService.TriggerIfNotRunning(HandleNextSubmissionJob.RecurringJobId);
        }

        return Task.CompletedTask;
    }
}