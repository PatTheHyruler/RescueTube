using System.Linq.Expressions;
using RescueTube.Core.Data.Specifications;
using RescueTube.Core.DTO.Settings;
using RescueTube.Domain;
using RescueTube.Domain.Entities;

namespace RescueTube.DAL.EF.Specifications;

public class SettingSpecification : ISettingSpecification
{
    public Expression<Func<IQueryable<Setting>, bool?>> GetSettingValue(SettingDefinition.Bool definition)
    {
        return GetSettingStructValueBase<bool, Setting.Bool>(definition);
    }

    public Expression<Func<IQueryable<Setting>, bool>> GetSettingValue(SettingDefinition.Bool.WithDefault definition)
    {
        return GetSettingStructValueBase<bool, Setting.Bool>(definition);
    }

    public Expression<Func<IQueryable<Setting>, long?>> GetSettingValue(SettingDefinition.Long definition)
    {
        return GetSettingStructValueBase<long, Setting.Long>(definition);
    }

    public Expression<Func<IQueryable<Setting>, long>> GetSettingValue(SettingDefinition.Long.WithDefault definition)
    {
        return GetSettingStructValueBase<long, Setting.Long>(definition);
    }

    public Expression<Func<IQueryable<Setting>, string?>> GetSettingValue(SettingDefinition.String definition)
    {
        return GetSettingClassValueBase<string, Setting.String>(definition);
    }

    public Expression<Func<IQueryable<Setting>, string>> GetSettingValue(SettingDefinition.String.WithDefault definition)
    {
        return GetSettingClassValueBase<string, Setting.String>(definition);
    }

    public Expression<Func<IQueryable<Setting>, DataSize?>> GetSettingValue(SettingDefinition.DataSize definition)
    {
        return GetSettingStructValueBase<DataSize, Setting.DataSize>(definition);
    }

    public Expression<Func<IQueryable<Setting>, DataSize>> GetSettingValue(SettingDefinition.DataSize.WithDefault definition)
    {
        return GetSettingStructValueBase<DataSize, Setting.DataSize>(definition);
    }

    // Base methods below

    private static Expression<Func<IQueryable<Setting>, TValue?>> GetSettingStructValueBase<TValue, TSettingEntity>(
        ISettingDefinition<TValue> definition)
        where TSettingEntity : Setting<TValue>
        where TValue : struct
    {
        return settings => settings
            .OfType<TSettingEntity>()
            .Where(x => x.Key == definition.Key)
            .Select(x => x.Value)
            .Cast<TValue?>()
            .FirstOrDefault();
    }

    private static Expression<Func<IQueryable<Setting>, TValue?>> GetSettingClassValueBase<TValue, TSettingEntity>(
        ISettingDefinition<TValue> definition)
        where TSettingEntity : Setting<TValue>
        where TValue : class
    {
        return settings => settings
            .Where(x => x is TSettingEntity && x.Key == definition.Key).Cast<TSettingEntity>()
            .Select(x => x.Value)
            .FirstOrDefault();
    }

    private static Expression<Func<IQueryable<Setting>, TValue>> GetSettingStructValueBase<TValue, TSettingEntity>(
        ISettingDefinition<TValue>.IWithDefault definition)
        where TSettingEntity : Setting<TValue>
        where TValue : struct
    {
        return settings => settings
            .OfType<TSettingEntity>()
            .Where(x => x.Key == definition.Key)
            .Select(x => x.Value)
            .Cast<TValue?>()
            .FirstOrDefault() ?? definition.DefaultValue;
    }

    private static Expression<Func<IQueryable<Setting>, TValue>> GetSettingClassValueBase<TValue, TSettingEntity>(
        ISettingDefinition<TValue>.IWithDefault definition)
        where TSettingEntity : Setting<TValue>
        where TValue : class
    {
        return settings => settings
            .OfType<TSettingEntity>()
            .Where(x => x.Key == definition.Key)
            .Select(x => x.Value)
            .FirstOrDefault() ?? definition.DefaultValue;
    }
}