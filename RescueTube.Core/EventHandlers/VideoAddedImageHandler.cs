using System.Transactions;
using Hangfire;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RescueTube.Core.Events;
using RescueTube.Core.Jobs;
using RescueTube.Core.Utils;

namespace RescueTube.Core.EventHandlers;

// ReSharper disable once UnusedType.Global
public class VideoAddedImageHandler : INotificationHandler<VideoAddedEvent>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public VideoAddedImageHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task Handle(VideoAddedEvent notification, CancellationToken cancellationToken)
    {
        using var transaction =  TransactionUtils.NewTransactionScope(TransactionScopeOption.Required);
        using var scope = _serviceScopeFactory.CreateAsyncScope();
        var backgroundJobs = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();
        var downloadJobId = backgroundJobs.Enqueue<DownloadVideoImagesJob>(x =>
            x.DownloadVideoImages(notification.Id, default));
        backgroundJobs.ContinueJobWith<UpdateImagesResolutionJob>(downloadJobId,
            x => x.UpdateVideoImagesResolutionsAsync(notification.Id, default));
        transaction.Complete();
        return Task.CompletedTask;
    }
}