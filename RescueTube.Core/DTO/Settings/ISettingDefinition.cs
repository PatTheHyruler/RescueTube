namespace RescueTube.Core.DTO.Settings;

public interface ISettingDefinition<T> : ISettingDefinition
{
    public interface IWithDefault : ISettingDefinition<T>
    {
        T DefaultValue { get; }
    }
}

public interface ISettingDefinition
{
    string Key { get; }
}