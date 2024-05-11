using BLL.YouTube.Jobs;
using RescueTube.Core.Events.Events;
using Hangfire;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BLL.YouTube.EventHandlers;

public class VideoAddedCommentFetchHandler : INotificationHandler<VideoAddedEvent>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public VideoAddedCommentFetchHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task Handle(VideoAddedEvent notification, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateAsyncScope();
        var backgroundJobClient = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();
        backgroundJobClient.Enqueue<FetchCommentsJob>(x =>
            x.FetchVideoComments(notification.IdOnPlatform, default));
        
        return Task.CompletedTask;
    }
}