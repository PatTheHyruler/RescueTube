using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RescueTube.Core.Data;
using RescueTube.Core.DTO.Settings;
using RescueTube.Core.Errors;
using RescueTube.Core.Utils;
using RescueTube.Domain;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Services;

public class SettingService
{
    private readonly IDataUow _dataUow;
    private readonly SettingRegistry _settingRegistry;

    public SettingService(IDataUow dataUow, IOptions<SettingRegistry> settingRegistry)
    {
        _dataUow = dataUow;
        _settingRegistry = settingRegistry.Value;
    }

    private static async Task<T?> GetStructValueAsync<T>(
        IQueryable<Setting<T>> query, ISettingDefinition<T> settingDefinition, CancellationToken ct) where T : struct
    {
        return await query
            .Where(x => x.Key == settingDefinition.Key)
            .Select(x => x.Value)
            .AsNullable()
            .FirstOrDefaultAsync(ct);
    }

    private static async Task<T?> GetClassValueAsync<T>(
        IQueryable<Setting<T>> query, SettingDefinition<T> settingDefinition, CancellationToken ct) where T : class
    {
        return await query
            .Where(x => x.Key == settingDefinition.Key)
            .Select(x => x.Value)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<long?> GetValueAsync(SettingDefinition.Long settingDefinition, CancellationToken ct)
    {
        return await GetStructValueAsync(_dataUow.Ctx.LongSettings, settingDefinition, ct);
    }

    public async Task<bool?> GetValueAsync(SettingDefinition.Bool settingDefinition, CancellationToken ct)
    {
        return await GetStructValueAsync(_dataUow.Ctx.BoolSettings, settingDefinition, ct);
    }

    public async Task<string?> GetValueAsync(SettingDefinition.String settingDefinition, CancellationToken ct)
    {
        return await GetClassValueAsync(_dataUow.Ctx.StringSettings, settingDefinition, ct);
    }

    public async Task<DataSize?> GetValueAsync(SettingDefinition.DataSize settingDefinition, CancellationToken ct)
    {
        return await GetStructValueAsync(_dataUow.Ctx.DataSizeSettings, settingDefinition, ct);
    }

    public async Task<SettingValue[]> GetGeneralSettingsAsync(CancellationToken ct)
    {
        var settingDefinitions = _settingRegistry.SettingDefinitions;
        var keys = settingDefinitions.Select(x => x.Key);
        var settings = await _dataUow.Ctx.Settings.Where(x => keys.Contains(x.Key)).ToArrayAsync(ct);
        return settingDefinitions.GroupJoin(settings,
                definition => definition.Key,
                setting => setting.Key,
                static (definition, settings) => MapToSettingValue(definition, settings))
            .ToArray();
    }

    private static SettingValue MapToSettingValue(ISettingDefinition settingDefinition, IEnumerable<Setting> settings)
    {
        return settingDefinition switch
        {
            SettingDefinition.Long d => new SettingValue.Long(d, GetSetting<Setting.Long>(settings)?.Value),
            SettingDefinition.Bool d => new SettingValue.Bool(d, GetSetting<Setting.Bool>(settings)?.Value),
            SettingDefinition.String d => new SettingValue.String(d, GetSetting<Setting.String>(settings)?.Value),
            SettingDefinition.DataSize d => new SettingValue.DataSize(d, GetSetting<Setting.DataSize>(settings)?.Value),
            _ => throw new ArgumentOutOfRangeException(nameof(settingDefinition), settingDefinition, "Unrecognized setting definition"),
        };

        static TSetting? GetSetting<TSetting>(IEnumerable<Setting> settings) where TSetting : Setting => settings
            .OfType<TSetting>()
            .OrderByDescending(x => x.Id)
            .FirstOrDefault();
    }

    public Task<Result<Setting.Long, SettingDefinitionNotFoundError>> SetValueAsync(string key, long value, CancellationToken ct)
    {
        return SetValueAsync<long, Setting.Long, SettingDefinition.Long>(key, value, static (key, value) => new()
        {
            Key = key,
            Value = value,
        }, ct);
    }

    public Task<Result<Setting.Bool, SettingDefinitionNotFoundError>> SetValueAsync(string key, bool value, CancellationToken ct)
    {
        return SetValueAsync<bool, Setting.Bool, SettingDefinition.Bool>(key, value, static (key, value) => new()
        {
            Key = key,
            Value = value,
        }, ct);
    }

    public Task<Result<Setting.String, SettingDefinitionNotFoundError>> SetValueAsync(string key, string value, CancellationToken ct)
    {
        return SetValueAsync<string, Setting.String, SettingDefinition.String>(key, value, static (key, value) => new()
        {
            Key = key,
            Value = value,
        }, ct);
    }

    public Task<Result<Setting.DataSize, SettingDefinitionNotFoundError>> SetValueAsync(string key, DataSize value, CancellationToken ct)
    {
        return SetValueAsync<DataSize, Setting.DataSize, SettingDefinition.DataSize>(key, value, static (key, value) => new()
        {
            Key = key,
            Value = value,
        }, ct);
    }

    private async Task<Result<TSetting, SettingDefinitionNotFoundError>> SetValueAsync<TValue, TSetting, TSettingDefinition>(string key, TValue value,
        Func<string, TValue, TSetting> createSetting, CancellationToken ct)
        where TSetting : Setting<TValue>
        where TSettingDefinition : SettingDefinition<TValue>
    {
        var definition = _settingRegistry.SettingDefinitions
            .OfType<TSettingDefinition>()
            .FirstOrDefault(x => x.Key == key);
        if (definition is null)
        {
            return new SettingDefinitionNotFoundError(key, typeof(TSetting));
        }

        var dbSet = _dataUow.Ctx.Set<TSetting>();
        var setting = await dbSet.FirstOrDefaultAsync(x => x.Key == key, ct);
        if (setting is null)
        {
            setting = createSetting(key, value);
            dbSet.Add(setting);
        }
        else
        {
            setting.Value = value;
        }

        return setting;
    }
}