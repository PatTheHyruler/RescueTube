using Microsoft.Extensions.Logging;
using RescueTube.Core.Constants;
using RescueTube.Core.Utils;
using RescueTube.Domain;

namespace RescueTube.Core.Services;

public class StorageLimitService
{
    private readonly AppPaths _appPaths;
    private readonly ILogger<StorageLimitService> _logger;
    private readonly SettingService _settingService;

    public StorageLimitService(AppPaths appPaths, ILogger<StorageLimitService> logger, SettingService settingService)
    {
        _appPaths = appPaths;
        _logger = logger;
        _settingService = settingService;
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

    public async Task<bool> IsVideoDownloadForbiddenAsync(CancellationToken ct)
    {
        // TODO: Cache these maybe?
        var videosBaseDirectory = _appPaths.GetVideosBaseDirectory();
        if (!Directory.Exists(videosBaseDirectory))
        {
            Directory.CreateDirectory(videosBaseDirectory);
        }
        var driveInfo = new DriveInfo(_appPaths.GetAbsolutePathFromContentRoot(videosBaseDirectory));

        await WaitForDriveToBeReady(driveInfo, ct);
        var availableFreeSpace = DataSize.FromBytes(driveInfo.AvailableFreeSpace);

        var minFreeSpace = await _settingService.GetValueAsync(SettingDefinitions.MinFreeSpaceForVideoDownload, ct)
                           ?? SettingDefinitions.MinFreeSpaceForVideoDownload.DefaultValue;

        _logger.LogInformation(
            "Available free space: {AvailableFreeSpace}, minimum required: {MinimumRequiredFreeSpace}",
            availableFreeSpace,
            minFreeSpace);
        return availableFreeSpace < minFreeSpace;
    }
}