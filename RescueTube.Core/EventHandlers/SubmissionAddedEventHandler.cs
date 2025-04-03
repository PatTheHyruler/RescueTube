using Hangfire;
using MediatR;
using RescueTube.Core.Events;
using RescueTube.Core.Jobs;

namespace RescueTube.Core.EventHandlers;

public class SubmissionAddedEventHandler : INotificationHandler<SubmissionAddedEvent>
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public SubmissionAddedEventHandler(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public Task Handle(SubmissionAddedEvent notification, CancellationToken cancellationToken)
    {
        if (notification is { AutoSubmit: true })
        {
            _backgroundJobClient.Enqueue<HandleSubmissionJob>(x => x.HandleSubmissionAsync(notification.SubmissionId, CancellationToken.None));
        }

        return Task.CompletedTask;
    }
}