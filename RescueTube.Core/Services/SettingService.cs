using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RescueTube.Core.Data;
using RescueTube.Core.DTO.Settings;
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
        IQueryable<Setting<T>> query, SettingDefinition<T> settingDefinition, CancellationToken ct) where T : struct
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

    private static SettingValue MapToSettingValue(SettingDefinition settingDefinition, IEnumerable<Setting> settings)
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

    public enum SettingUpdateResult
    {
        Success,
        DefinitionNotFound,
        DefinitionTypeMismatch,
    }

    public async Task<IReadOnlyDictionary<string, SettingUpdateResult>> UpdateSettingsAsync(IEnumerable<SettingValueUpdateDto> settingValues, CancellationToken ct)
    {
        var definitionsWithUpdates = settingValues
            .GroupJoin(
                _settingRegistry.SettingDefinitions,
                x => x.Key,
                x => x.Key,
                (update, definitions) => (Update: update, Definition: definitions.SingleOrDefault()))
            .ToArray();
        var keys = definitionsWithUpdates.Select(x => x.Update.Key);
        var settings = await _dataUow.Ctx.Settings
            .Where(x => keys.Contains(x.Key))
            .ToArrayAsync(ct);

        var results = new Dictionary<string, SettingUpdateResult>(definitionsWithUpdates.Length);
        foreach (var (baseUpdateDto, definition) in definitionsWithUpdates)
        {
            var result = baseUpdateDto switch
            {
                SettingValueUpdateDto.Long updateDto => HandleSettingUpdate<Setting.Long, SettingValueUpdateDto.Long, SettingDefinition.Long, long>(updateDto, definition),
                SettingValueUpdateDto.Bool updateDto => HandleSettingUpdate<Setting.Bool, SettingValueUpdateDto.Bool, SettingDefinition.Bool, bool>(updateDto, definition),
                SettingValueUpdateDto.String updateDto => HandleSettingUpdate<Setting.String, SettingValueUpdateDto.String, SettingDefinition.String, string>(updateDto, definition),
                SettingValueUpdateDto.DataSize updateDto => HandleSettingUpdate<Setting.DataSize, SettingValueUpdateDto.DataSize, SettingDefinition.DataSize, DataSize>(updateDto, definition),
                _ => throw new SwitchExpressionException(baseUpdateDto),
            };
            results[baseUpdateDto.Key] = result;
        }

        return results;

        SettingUpdateResult HandleSettingUpdate<TSetting, TUpdateDto, TDefinition, T>(
            TUpdateDto updateDto,
            SettingDefinition? baseDefinition)
            where TSetting : Setting<T>, ICreatableSetting<TSetting, T>
            where TUpdateDto : SettingValueUpdateDto, ISettingValueUpdateDto<T?>
            where TDefinition : SettingDefinition<T>
        {
            if (baseDefinition is null)
            {
                return SettingUpdateResult.DefinitionNotFound;
            }

            if (baseDefinition is not TDefinition definition)
            {
                return SettingUpdateResult.DefinitionTypeMismatch;
            }

            var baseSettingEntity = settings.SingleOrDefault(x => x.Key == definition.Key);
            if (!updateDto.HasValue)
            {
                if (baseSettingEntity is not null)
                {
                    _dataUow.Ctx.Settings.Remove(baseSettingEntity);
                }

                return SettingUpdateResult.Success;
            }

            if (baseSettingEntity is TSetting settingEntity)
            {
                settingEntity.Value = updateDto.Value;
                return SettingUpdateResult.Success;
            }

            // Setting entity exists but has wrong type - remove it before creating new entity
            if (baseSettingEntity is not null)
            {
                _dataUow.Ctx.Settings.Remove(baseSettingEntity);
            }

            _dataUow.Ctx.Settings.Add(TSetting.Create(key: definition.Key, value: updateDto.Value));
            return SettingUpdateResult.Success;
        }
    }
}