using Domain.Enums;

namespace BLL;

public static class AppPaths
{
    private const string Videos = "videos";

    public static string GetVideosDirectory(EPlatform platform, AppPathOptions? options) =>
        Path.Combine(options.OrDefault().Downloads, Videos, platform.ToString());

    private static AppPathOptions OrDefault(this AppPathOptions? options) =>
        options ?? new AppPathOptions();
}

public class AppPathOptions
{
    public const string Section = "Paths";

    public string Downloads { get; set; } = "downloads";
}