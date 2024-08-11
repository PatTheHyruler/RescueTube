using Hangfire;
using MediatR;
using RescueTube.Core.Events;
using RescueTube.Domain.Enums;
using RescueTube.YouTube.Jobs;

namespace RescueTube.YouTube.EventHandlers;

public class AuthorArchivalEnabledEventHandler : INotificationHandler<AuthorArchivalEnabledEvent>
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public AuthorArchivalEnabledEventHandler(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public Task Handle(AuthorArchivalEnabledEvent notification, CancellationToken cancellationToken)
    {
        if (notification.Platform != EPlatform.YouTube || !notification.AuthorArchivalSettings.Active)
        {
            return Task.CompletedTask;
        }

        _backgroundJobClient.Enqueue<FetchAuthorVideosJob>(x =>
            x.FetchAuthorVideos(notification.AuthorId, false, default));

        return Task.CompletedTask;
    }
}