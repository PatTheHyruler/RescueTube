using BLL.YouTube.Jobs;
using RescueTube.Core.Events.Events;
using Domain.Enums;
using Hangfire;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BLL.YouTube.EventHandlers;

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