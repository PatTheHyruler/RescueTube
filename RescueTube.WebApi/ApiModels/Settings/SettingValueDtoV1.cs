using System.Text.Json.Serialization;

namespace RescueTube.WebApi.ApiModels.Settings;

[JsonDerivedType(typeof(Long), nameof(Long))]
[JsonDerivedType(typeof(Bool), nameof(Bool))]
[JsonDerivedType(typeof(String), nameof(String))]
[JsonDerivedType(typeof(DataSizeBytes), nameof(DataSizeBytes))]
public abstract record SettingValueDtoV1
{
    public sealed record Long : SettingValueStructDtoV1<long, SettingDefinitionDtoV1.Long>;
    public sealed record Bool : SettingValueStructDtoV1<bool, SettingDefinitionDtoV1.Bool>;
    public sealed record String : SettingValueClassDtoV1<string, SettingDefinitionDtoV1.String>;
    public sealed record DataSizeBytes : SettingValueStructDtoV1<long, SettingDefinitionDtoV1.DataSizeBytes>;
}

public abstract record SettingValueStructDtoV1<TValue, TSettingDefinition> : SettingValueDtoV1
    where TSettingDefinition : SettingDefinitionStructDtoV1<TValue> where TValue : struct
{
    public required TSettingDefinition Definition { get; init; }
    public required TValue? Value { get; init; }
}

public abstract record SettingValueClassDtoV1<TValue, TSettingDefinition> : SettingValueDtoV1
    where TSettingDefinition : SettingDefinitionClassDtoV1<TValue> where TValue : class
{
    public required TSettingDefinition Definition { get; init; }
    public required TValue? Value { get; init; }
}