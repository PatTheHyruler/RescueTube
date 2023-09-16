using System.Collections.Concurrent;
using BLL.Base;
using BLL.Utils;
using DAL.EF.DbContexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BLL.YouTube.BackgroundServices;

public class VideoDownloadBackgroundService : BaseBackgroundService
{
    private readonly ConcurrentQueue<Guid> _videoQueue;
    private readonly EventSignaller _signaller;

    public VideoDownloadBackgroundService(IServiceProvider services, ILogger<VideoDownloadBackgroundService> logger) :
        base(services, logger)
    {
        _signaller = new EventSignaller();
        _videoQueue = new ConcurrentQueue<Guid>();
    }

    private void OnVideoAdded(object? sender, PlatformEntityAddedEventArgs args)
    {
        _videoQueue.Enqueue(args.Id);
        _signaller.Signal();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (!_videoQueue.IsEmpty)
            {
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                if (stoppingToken.IsCancellationRequested) return;
                var batchSize = Math.Min(_videoQueue.Count, 10);
                var ids = new List<Guid>();
                for (var i = 0; i < batchSize && !_videoQueue.IsEmpty; i++)
                {
                    if (_videoQueue.TryDequeue(out var id))
                    {
                        ids.Add(id);
                    } else break;
                }

                await DownloadVideos(ids, stoppingToken);
            }
            else
            {
                await DownloadVideos(null, stoppingToken);
            }

            if (_videoQueue.IsEmpty)
            {
                await _signaller.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }

    private async Task DownloadVideos(IEnumerable<Guid>? videoIds, CancellationToken ct)
    {
        if (ct.IsCancellationRequested) return;
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AbstractAppDbContext>();
        var youTubeUow = scope.ServiceProvider.GetRequiredService<YouTubeUow>();

        await youTubeUow.VideoService.DownloadVideos(videoIds, ct);
        await dbContext.SaveChangesAsync(CancellationToken.None);  // Don't cancel since files might have been downloaded already
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        Context.VideoAdded += OnVideoAdded;
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        Context.VideoAdded -= OnVideoAdded;
        return base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _signaller.Dispose();
        base.Dispose();
    }
}