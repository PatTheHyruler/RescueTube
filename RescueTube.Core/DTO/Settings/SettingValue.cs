namespace RescueTube.Core.DTO.Settings;

public abstract record SettingValue
{
    public sealed record Long(SettingDefinition.Long Definition, long? Value)
        : SettingValueStruct<long, SettingDefinition.Long>(Definition, Value);

    public sealed record Bool(SettingDefinition.Bool Definition, bool? Value)
        : SettingValueStruct<bool, SettingDefinition.Bool>(Definition, Value);

    public sealed record String(SettingDefinition.String Definition, string? Value)
        : SettingValueClass<string, SettingDefinition.String>(Definition, Value);

    public sealed record DataSize(SettingDefinition.DataSize Definition, Domain.DataSize? Value)
        : SettingValueStruct<Domain.DataSize, SettingDefinition.DataSize>(Definition, Value);
}

public abstract record SettingValueStruct<TValue, TSettingDefinition>(TSettingDefinition Definition, TValue? Value)
    : SettingValue where TSettingDefinition : SettingDefinition<TValue> where TValue : struct;

public abstract record SettingValueClass<TValue, TSettingDefinition>(TSettingDefinition Definition, TValue? Value)
    : SettingValue where TSettingDefinition : SettingDefinition<TValue> where TValue : class;