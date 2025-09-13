using System.Text.Json.Serialization;

namespace WebApp.ApiModels.Settings;

[JsonDerivedType(typeof(Long), nameof(Long))]
[JsonDerivedType(typeof(Bool), nameof(Bool))]
[JsonDerivedType(typeof(String), nameof(String))]
[JsonDerivedType(typeof(DataSizeBytes), nameof(DataSizeBytes))]
public abstract record SettingValueUpdateDtoV1
{
    public required string Key { get; init; }

    public sealed record Long(long? Value) : SettingValueUpdateDtoV1;
    public sealed record Bool(bool? Value) : SettingValueUpdateDtoV1;
    public sealed record String(string? Value) : SettingValueUpdateDtoV1;
    public sealed record DataSizeBytes(long? Value) : SettingValueUpdateDtoV1;
}