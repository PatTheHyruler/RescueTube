namespace RescueTube.Core.DTO.Settings;

public interface ISettingDefinition<T>
{
    public string Key { get; }

    public interface IWithDefault : ISettingDefinition<T>
    {
        T DefaultValue { get; }
    }
}
