using Hangfire;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RescueTube.Core.Events;
using RescueTube.Core.Jobs;

namespace RescueTube.Core.EventHandlers;

public class VideoAddedImageHandler : INotificationHandler<VideoAddedEvent>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public VideoAddedImageHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task Handle(VideoAddedEvent notification, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateAsyncScope();
        var backgroundJobs = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();
        backgroundJobs.Enqueue<DownloadVideoImagesJob>(x =>
            x.DownloadVideoImages(notification.Id, default));
        return Task.CompletedTask;
    }
}