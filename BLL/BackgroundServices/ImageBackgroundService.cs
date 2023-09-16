using BLL.Base;
using BLL.Utils;
using DAL.EF.DbContexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BLL.BackgroundServices;

public class ImageBackgroundService : BaseBackgroundService
{
    private ulong _potentialNewImagesAdded = 1; // Starts at 1 to force a check at startup
    private readonly EventSignaller _signaller;

    private static readonly TimeSpan MaxTaskPeriod = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan MinTaskPeriod = TimeSpan.FromSeconds(5);

    public ImageBackgroundService(IServiceProvider services, ILogger<ImageBackgroundService> logger) :
        base(services, logger)
    {
        _signaller = new EventSignaller();
    }

    private void OnPotentialNewImages(object? sender, EventArgs args)
    {
        Interlocked.Increment(ref _potentialNewImagesAdded);
        _signaller.Signal();
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        Context.VideoAdded += OnPotentialNewImages;
        Context.AuthorAdded += OnPotentialNewImages;
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        Context.VideoAdded -= OnPotentialNewImages;
        Context.AuthorAdded -= OnPotentialNewImages;
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (Interlocked.Read(ref _potentialNewImagesAdded) > 0)
            {
                await Task.Delay(MinTaskPeriod, ct).ContinueWith(_ => { }, CancellationToken.None); // TODO: Find better way to do this
                Interlocked.Exchange(ref _potentialNewImagesAdded, 0);
                await DownloadImagesAsync(ct);
                continue;
            }

            await _signaller.Delay(MaxTaskPeriod, ct);
        }
    }

    private async Task DownloadImagesAsync(CancellationToken ct)
    {
        if (ct.IsCancellationRequested) return;
        using var logScope = Logger.BeginScope("Downloading images");
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AbstractAppDbContext>();

        var serviceUow = scope.ServiceProvider.GetRequiredService<ServiceUow>();
        await serviceUow.ImageService.DownloadNotDownloadedImages(ct);

        await dbContext.SaveChangesAsync(ct);
    }

    public override void Dispose()
    {
        _signaller.Dispose();
        base.Dispose();
    }
}