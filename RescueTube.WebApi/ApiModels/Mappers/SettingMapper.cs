using RescueTube.Core.DTO.Settings;
using RescueTube.WebApi.ApiModels.Settings;
using Riok.Mapperly.Abstractions;

namespace RescueTube.WebApi.ApiModels.Mappers;

[Mapper]
public static partial class SettingMapper
{
    [MapDerivedType<SettingValue.Long, SettingValueDtoV1.Long>]
    [MapDerivedType<SettingValue.Bool, SettingValueDtoV1.Bool>]
    [MapDerivedType<SettingValue.String, SettingValueDtoV1.String>]
    [MapDerivedType<SettingValue.DataSize, SettingValueDtoV1.DataSizeBytes>]
    public static partial SettingValueDtoV1 MapToSettingValueDtoV1(this SettingValue settingValue);

    [MapDerivedType<SettingDefinition.Long, SettingDefinitionDtoV1.Long>]
    [MapDerivedType<SettingDefinition.Bool, SettingDefinitionDtoV1.Bool>]
    [MapDerivedType<SettingDefinition.String, SettingDefinitionDtoV1.String>]
    [MapDerivedType<SettingDefinition.DataSize, SettingDefinitionDtoV1.DataSizeBytes>]
    [MapProperty(nameof(SettingDefinition.Long.OptionalDefaultValue), nameof(SettingDefinitionDtoV1.Long.DefaultValue))]
    private static partial SettingDefinitionDtoV1 MapToSettingDefinitionDtoV1(this SettingDefinition settingDefinition);

    [MapDerivedType<SettingValueUpdateDtoV1.Long, SettingValueUpdateDto.Long>]
    [MapDerivedType<SettingValueUpdateDtoV1.Bool, SettingValueUpdateDto.Bool>]
    [MapDerivedType<SettingValueUpdateDtoV1.String, SettingValueUpdateDto.String>]
    [MapDerivedType<SettingValueUpdateDtoV1.DataSizeBytes, SettingValueUpdateDto.DataSize>]
    public static partial SettingValueUpdateDto MapToCoreSettingValueUpdateDto(this SettingValueUpdateDtoV1 dto);
}