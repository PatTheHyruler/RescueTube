using Hangfire;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RescueTube.Core.Events;
using RescueTube.Core.Jobs;

namespace RescueTube.Core.EventHandlers;

// ReSharper disable once UnusedType.Global
public class AuthorAddedImageHandler : INotificationHandler<AuthorAddedEvent>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public AuthorAddedImageHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task Handle(AuthorAddedEvent notification, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateAsyncScope();
        var backgroundJobs = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();
        var downloadJobId = backgroundJobs.Enqueue<DownloadAuthorImagesJob>(x => 
            x.DownloadAuthorImages(notification.Id, default));
        backgroundJobs.ContinueJobWith<UpdateImagesResolutionJob>(downloadJobId,
            x => x.UpdateAuthorImagesResolutionsAsync(notification.Id, default));
        return Task.CompletedTask;
    }
}