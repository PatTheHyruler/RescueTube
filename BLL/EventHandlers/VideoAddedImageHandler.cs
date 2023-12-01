using BLL.Events.Events;
using BLL.Jobs;
using Hangfire;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BLL.EventHandlers;

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