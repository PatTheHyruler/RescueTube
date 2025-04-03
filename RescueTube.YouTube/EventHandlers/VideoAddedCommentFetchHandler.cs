using MediatR;
using RescueTube.Core.Events;
using RescueTube.Domain.Enums;

namespace RescueTube.YouTube.EventHandlers;

public class VideoAddedCommentFetchHandler : INotificationHandler<VideoAddedEvent>
{
    public Task Handle(VideoAddedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.Platform != EPlatform.YouTube)
        {
            return Task.CompletedTask;
        }

        // TODO: Enqueue video comments fetch

        return Task.CompletedTask;
    }
}