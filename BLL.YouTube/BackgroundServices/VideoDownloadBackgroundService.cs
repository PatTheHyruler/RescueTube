using System.Threading.Channels;
using BLL.Base;
using DAL.EF.DbContexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BLL.YouTube.BackgroundServices;

public class VideoDownloadBackgroundService : BaseBackgroundService
{
    private readonly Channel<Guid> _videoQueue;

    public VideoDownloadBackgroundService(IServiceProvider services, ILogger<VideoDownloadBackgroundService> logger) :
        base(services, logger)
    {
        _videoQueue = Channel.CreateUnbounded<Guid>();
    }

    private void OnVideoAdded(object? sender, PlatformEntityAddedEventArgs args)
    {
        _videoQueue.Writer.WriteAsync(args.Id); // Fire and forget
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // TODO: All of this
        throw new NotImplementedException();
    }

    private async Task DownloadVideos(IEnumerable<Guid>? videoIds, CancellationToken ct)
    {
        if (ct.IsCancellationRequested) return;
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AbstractAppDbContext>();
        var youTubeUow = scope.ServiceProvider.GetRequiredService<YouTubeUow>();

        await youTubeUow.VideoService.DownloadVideos(videoIds, ct);
        // ReSharper disable once MethodSupportsCancellation Don't cancel since files might have been downloaded already
        await dbContext.SaveChangesAsync();
    }
}