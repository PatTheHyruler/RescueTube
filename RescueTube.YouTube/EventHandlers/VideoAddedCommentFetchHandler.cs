using Hangfire;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RescueTube.Core.Events.Events;
using RescueTube.YouTube.Jobs;

namespace RescueTube.YouTube.EventHandlers;

public class VideoAddedCommentFetchHandler : INotificationHandler<VideoAddedEvent>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public VideoAddedCommentFetchHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task Handle(VideoAddedEvent notification, CancellationToken cancellationToken)
    {
        // TODO: Re-enable once comments job has been reworked
        // using var scope = _serviceScopeFactory.CreateAsyncScope();
        // var backgroundJobClient = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();
        // backgroundJobClient.Enqueue<FetchCommentsJob>(x =>
        //     x.FetchVideoComments(notification.IdOnPlatform, default));
        
        return Task.CompletedTask;
    }
}