using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Constants;
using RescueTube.Core.Data;
using RescueTube.Core.Services;

namespace RescueTube.Core.Jobs;

public class DataFetchesKillSwitchJob
{
    private readonly IDataUow _dataUow;
    private readonly TimeProvider _timeProvider;
    private readonly SettingService _settingService;
    private readonly ILogger<DataFetchesKillSwitchJob> _logger;

    public DataFetchesKillSwitchJob(IDataUow dataUow, TimeProvider timeProvider, SettingService settingService, ILogger<DataFetchesKillSwitchJob> logger)
    {
        _dataUow = dataUow;
        _timeProvider = timeProvider;
        _settingService = settingService;
        _logger = logger;
    }

    public async Task CheckDataFetchAmountAsync(CancellationToken ct)
    {
        var now = _timeProvider.GetUtcNow();
        var offsetMinutes =
            await _settingService.GetValueAsync(SettingDefinitions.DataFetchKillSwitchCutoffMinutes, ct);
        var cutoffFrom = now.Subtract(TimeSpan.FromMinutes(-Math.Abs(offsetMinutes)));
        var dataFetchCount = await _dataUow.Ctx.DataFetches
            .Where(x => x.OccurredAt >= cutoffFrom)
            .CountAsync(ct);

        var dataFetchLimit = await _settingService.GetValueAsync(SettingDefinitions.DataFetchKillSwitchLimit, ct);
        _logger.LogInformation("Data fetches since {CutoffFrom}: {DataFetchCount}, limit: {DataFetchLimit}",
            cutoffFrom, dataFetchCount, dataFetchLimit);
        if (dataFetchCount >= dataFetchLimit)
        {
            await _settingService.UpdateSettingAsync(SettingDefinitions.DisableAllArchival, true, ct);
            // TODO: Add notification support & store reason for disabling
        }
    }
}