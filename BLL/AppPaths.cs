using System.Text;
using Domain.Entities;
using Domain.Enums;

namespace BLL;

public static class AppPaths
{
    private const string Videos = "videos";
    private const string Images = "images";

    public static string GetVideosDirectory(EPlatform platform, AppPathOptions? options) =>
        Path.Combine(options.OrDefault().Downloads, Videos, platform.ToString());

    public static string GetImagesDirectory(EPlatform platform, AppPathOptions? options) =>
        Path.Combine(options.OrDefault().Downloads, Images, platform.ToString());

    public static string ToFileNameSanitized(this string str, int? maxLength = null)
    {
        foreach (var disallowed in Path.GetInvalidFileNameChars())
        {
            str = str.Replace(disallowed.ToString(), "");
        }

        if (maxLength == null || str.Length <= maxLength) return str;
        return str[..maxLength.Value];
    }

    private static AppPathOptions OrDefault(this AppPathOptions? options) =>
        options ?? new AppPathOptions();
}

public class AppPathOptions
{
    public const string Section = "Paths";

    public string Downloads { get; set; } = "downloads";
}