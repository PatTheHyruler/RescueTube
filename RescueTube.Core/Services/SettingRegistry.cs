using RescueTube.Core.DTO.Settings;

namespace RescueTube.Core.Services;

public class SettingRegistry
{
    private readonly HashSet<ISettingDefinition> _settingDefinitions = [];
    public IReadOnlySet<ISettingDefinition> SettingDefinitions => _settingDefinitions;

    public void RegisterDefinitions(params IEnumerable<ISettingDefinition> settingDefinitions)
    {
        foreach (var settingDefinition in settingDefinitions)
        {
            _settingDefinitions.Add(settingDefinition);
        }
    }
}