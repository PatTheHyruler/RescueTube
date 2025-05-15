namespace RescueTube.Core.DTO.Settings;

public interface ISettingDefinition<T>
{
    public interface IWithDefault : ISettingDefinition<T>
    {
        T DefaultValue { get; }
    }
}
