using Microsoft.Extensions.Logging;
using RescueTube.Core.Utils;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.Services;

public class StorageLimitService
{
    private readonly AppPaths _appPaths;
    private readonly ILogger<StorageLimitService> _logger;

    public StorageLimitService(AppPaths appPaths, ILogger<StorageLimitService> logger)
    {
        _appPaths = appPaths;
        _logger = logger;
    }

    private static async Task WaitForDriveToBeReady(DriveInfo di, CancellationToken ct = default)
    {
        var elapsed = false;

        var timer = new System.Timers.Timer(TimeSpan.FromSeconds(20));
        timer.Enabled = true;
        timer.AutoReset = false;

        timer.Elapsed += (_, _) => elapsed = true;

        while (!di.IsReady && !ct.IsCancellationRequested && !elapsed)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), ct);
        }

        if (!di.IsReady)
        {
            throw new OperationCanceledException("The drive is not ready");
        }
    }

    public async Task<bool> IsVideoDownloadForbidden(EPlatform platform, CancellationToken ct = default)
    {
        // TODO: Allow configuring this. Globally, per platform, per video type, per author???
        const long minFreeSpace = 400L * 1024 * 1024 * 1024;

        // TODO: Allow caching this maybe?
        var driveInfo = new DriveInfo(_appPaths.GetAbsolutePathFromContentRoot(_appPaths.GetVideosDirectory(platform)));

        await WaitForDriveToBeReady(driveInfo, ct);

        _logger.LogInformation(
            "Available free space: {AvailableFreeSpace}, minimum required: {MinimumRequiredFreeSpace}",
            DataFormatUtils.GetHumanReadableFormat(driveInfo.AvailableFreeSpace),
            DataFormatUtils.GetHumanReadableFormat(minFreeSpace));
        return driveInfo.AvailableFreeSpace < minFreeSpace;
    }
}