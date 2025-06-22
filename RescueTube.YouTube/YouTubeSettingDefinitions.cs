using System.Collections.Frozen;
using RescueTube.Core.DTO.Settings;

namespace RescueTube.YouTube;

public static class YouTubeSettingDefinitions
{
    public static readonly SettingDefinition.Bool.WithDefault UseCookieFile =
        new("YouTube:UseCookieFile", false);

    public static readonly FrozenSet<SettingDefinition> AllDefinitions = [
        UseCookieFile,
    ];
}