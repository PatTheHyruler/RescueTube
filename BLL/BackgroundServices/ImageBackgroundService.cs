using BLL.Base;
using DAL.EF.DbContexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BLL.BackgroundServices;

public class ImageBackgroundService : BaseBackgroundService
{
    private ulong _potentialNewImagesAdded = 1; // Starts at 1 to force a check at startup
    private CancellationTokenSource _potentialNewImagesAddedCancellationTokenSource;

    private static readonly TimeSpan MaxTaskPeriod = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan MinTaskPeriod = TimeSpan.FromSeconds(5);

    public ImageBackgroundService(IServiceProvider services, ILogger<ImageBackgroundService> logger) :
        base(services, logger)
    {
        _potentialNewImagesAddedCancellationTokenSource = new CancellationTokenSource();
    }

    private void OnPotentialNewImages(object? sender, EventArgs args)
    {
        Interlocked.Increment(ref _potentialNewImagesAdded);
        _potentialNewImagesAddedCancellationTokenSource.Cancel();
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
                await Task.Delay(MinTaskPeriod, ct);
                Interlocked.Exchange(ref _potentialNewImagesAdded, 0);
                await DownloadImagesAsync(ct);
                continue;
            }

            using var cts =
                CancellationTokenSource.CreateLinkedTokenSource(ct,
                    _potentialNewImagesAddedCancellationTokenSource.Token);
            await Task.Delay(MaxTaskPeriod, cts.Token);
            if (_potentialNewImagesAddedCancellationTokenSource.IsCancellationRequested)
            {
                _potentialNewImagesAddedCancellationTokenSource.Dispose();
                _potentialNewImagesAddedCancellationTokenSource = new CancellationTokenSource();
                // TODO: Check if the OnPotentialNewImages callbacks actually reference this new cts.
            }
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
        _potentialNewImagesAddedCancellationTokenSource.Dispose();
        base.Dispose();
    }
}