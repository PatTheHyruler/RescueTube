using Hangfire;
using MediatR;
using RescueTube.Core.Events;
using RescueTube.Domain.Enums;
using RescueTube.YouTube.Jobs;

namespace RescueTube.YouTube.EventHandlers;

public class SubmissionAddedEventHandler : INotificationHandler<SubmissionAddedEvent>
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public SubmissionAddedEventHandler(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public Task Handle(SubmissionAddedEvent notification, CancellationToken cancellationToken)
    {
        if (notification is { AutoSubmit: true, Platform: EPlatform.YouTube })
        {
            _backgroundJobClient.Enqueue<HandleSubmissionJob>(x => x.RunAsync(notification.SubmissionId, default));
        }

        return Task.CompletedTask;
    }
}