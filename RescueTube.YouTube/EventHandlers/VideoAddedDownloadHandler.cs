using Hangfire;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RescueTube.Core.Events.Events;
using RescueTube.Domain.Enums;
using RescueTube.YouTube.Jobs;

namespace RescueTube.YouTube.EventHandlers;

public class VideoAddedDownloadHandler : INotificationHandler<VideoAddedEvent>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public VideoAddedDownloadHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task Handle(VideoAddedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.Platform != EPlatform.YouTube)
        {
            return Task.CompletedTask;
        }

        using var scope = _serviceScopeFactory.CreateAsyncScope();
        var backgroundJobs = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();
        backgroundJobs.Enqueue<DownloadVideoJob>(x => 
            x.DownloadVideo(notification.Id, default));

        return Task.CompletedTask;
    }
}