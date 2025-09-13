namespace RescueTube.WebApi.ApiModels.Settings;

public abstract record SettingDefinitionDtoV1
{
    public required string Key { get; init; }

    public record Long : SettingDefinitionStructDtoV1<long>;
    public record Bool : SettingDefinitionStructDtoV1<bool>;
    public record String : SettingDefinitionClassDtoV1<string>;
    public record DataSizeBytes : SettingDefinitionStructDtoV1<long>;
}

public abstract record SettingDefinitionStructDtoV1<T> : SettingDefinitionDtoV1 where T : struct
{
    public required T? DefaultValue { get; init; }
}

public abstract record SettingDefinitionClassDtoV1<T> : SettingDefinitionDtoV1 where T : class
{
    public required T? DefaultValue { get; init; }
}