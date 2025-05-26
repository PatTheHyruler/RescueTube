using System.Linq.Expressions;
using RescueTube.Core.DTO.Settings;
using RescueTube.Domain;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Data.Specifications;

public interface ISettingSpecification
{
    Expression<Func<IQueryable<Setting>, bool?>> GetSettingValue(SettingDefinition.Bool definition);
    Expression<Func<IQueryable<Setting>, bool>> GetSettingValue(SettingDefinition.Bool.WithDefault definition);

    Expression<Func<IQueryable<Setting>, long?>> GetSettingValue(SettingDefinition.Long definition);
    Expression<Func<IQueryable<Setting>, long>> GetSettingValue(SettingDefinition.Long.WithDefault definition);

    Expression<Func<IQueryable<Setting>, string?>> GetSettingValue(SettingDefinition.String definition);
    Expression<Func<IQueryable<Setting>, string>> GetSettingValue(SettingDefinition.String.WithDefault definition);

    Expression<Func<IQueryable<Setting>, DataSize?>> GetSettingValue(SettingDefinition.DataSize definition);
    Expression<Func<IQueryable<Setting>, DataSize>> GetSettingValue(SettingDefinition.DataSize.WithDefault definition);
}