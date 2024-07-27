using Hangfire;
using MediatR;
using RescueTube.Core.Events;
using RescueTube.Domain.Enums;
using RescueTube.YouTube.Jobs;

namespace RescueTube.YouTube.EventHandlers;

// ReSharper disable once UnusedType.Global
public class AuthorAddedFetchYouTubeExplodeHandler : INotificationHandler<AuthorAddedEvent>
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public AuthorAddedFetchYouTubeExplodeHandler(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public Task Handle(AuthorAddedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.Platform != EPlatform.YouTube)
        {
            return Task.CompletedTask;
        }

        _backgroundJobClient.Enqueue<FetchYouTubeExplodeAuthorDataJob>(x =>
            x.FetchYouTubeExplodeAuthorData(notification.Id, default));

        return Task.CompletedTask;
    }
}