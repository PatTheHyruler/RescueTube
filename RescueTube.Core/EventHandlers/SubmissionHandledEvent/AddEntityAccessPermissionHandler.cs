using Hangfire;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RescueTube.Core.Jobs;

namespace RescueTube.Core.EventHandlers.SubmissionHandledEvent;

public class AddEntityAccessPermissionHandler : INotificationHandler<Events.SubmissionHandledEvent>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public AddEntityAccessPermissionHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task Handle(Events.SubmissionHandledEvent notification, CancellationToken cancellationToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var backgroundJobs = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();
        backgroundJobs.Enqueue<SubmissionAddEntityAccessPermissionJob>(x => x.RunAsync(notification.SubmissionId, default));
    }
}