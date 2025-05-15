namespace RescueTube.Core.DTO.Settings;

public abstract record SettingDefinition(string Key)
{
    public record Long(string Key) : SettingDefinitionForStruct<long>(Key)
    {
        public sealed record WithDefault(string Key, long DefaultValue) : Long(Key), ISettingDefinition<long>.IWithDefault;
    }

    public record Bool(string Key) : SettingDefinitionForStruct<bool>(Key)
    {
        public sealed record WithDefault(string Key, bool DefaultValue) : Bool(Key), ISettingDefinition<bool>.IWithDefault;
    }

    public record String(string Key) : SettingDefinitionForClass<string>(Key)
    {
        public sealed record WithDefault(string Key, string DefaultValue) : String(Key), ISettingDefinition<string>.IWithDefault;
    }

    public record DataSize(string Key) : SettingDefinitionForStruct<Domain.DataSize>(Key)
    {
        public sealed record WithDefault(string Key, Domain.DataSize DefaultValue) : DataSize(Key), ISettingDefinition<Domain.DataSize>.IWithDefault;
    }
}

public abstract record SettingDefinition<T>(string Key) : SettingDefinition(Key), ISettingDefinition<T>;

public abstract record SettingDefinitionForStruct<T>(string Key) : SettingDefinition<T>(Key) where T : struct
{
    public T? OptionalDefaultValue => this is ISettingDefinition<T>.IWithDefault def ? def.DefaultValue : null;
}

public abstract record SettingDefinitionForClass<T>(string Key) : SettingDefinition<T>(Key) where T : class
{
    public T? OptionalDefaultValue => this is ISettingDefinition<T>.IWithDefault def ? def.DefaultValue : null;
}