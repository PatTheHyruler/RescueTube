using System.Collections.Frozen;
using RescueTube.Core.DTO.Settings;
using RescueTube.Domain;

namespace RescueTube.Core.Constants;

public static class SettingDefinitions
{
    public static readonly SettingDefinition.DataSize.WithDefault MinFreeSpaceForVideoDownload =
        new("MinFreeSpaceForVideoDownload", DataSize.FromGibibytes(400));

    public static readonly SettingDefinition.Bool.WithDefault DisableAllArchival =
        new("DisableAllArchival", false);

    public static readonly FrozenSet<SettingDefinition> AllDefinitions = [
        MinFreeSpaceForVideoDownload,
        DisableAllArchival,
    ];
}