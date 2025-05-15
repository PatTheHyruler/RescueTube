using RescueTube.Core.DTO.Settings;

namespace RescueTube.Core.Services;

public class SettingRegistry
{
    private readonly HashSet<SettingDefinition> _settingDefinitions = [];
    public IReadOnlySet<SettingDefinition> SettingDefinitions => _settingDefinitions;

    public void RegisterDefinitions(params IEnumerable<SettingDefinition> settingDefinitions)
    {
        foreach (var settingDefinition in settingDefinitions)
        {
            _settingDefinitions.Add(settingDefinition);
        }
    }
}