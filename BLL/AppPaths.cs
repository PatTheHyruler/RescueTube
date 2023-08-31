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

    public static string MakeRelativeFilePath(string filePath, string? directory = null)
    {
        var fileUri = new Uri(filePath);
        directory ??= Directory.GetCurrentDirectory();
        var referenceUri = new Uri(directory);
        var relativePath = Uri.UnescapeDataString(referenceUri.MakeRelativeUri(fileUri).ToString());

        var directoryName = Path.GetFileName(directory);
        if (relativePath.StartsWith(directoryName))
        {
            relativePath = relativePath[directoryName.Length..];
            while (relativePath.StartsWith('/'))
            {
                relativePath = relativePath[1..];
            }
        }

        return relativePath.Replace('/', Path.DirectorySeparatorChar);
    }

    public static string? GuessImageFileExtensionFromMediaType(string? mediaType) =>
        mediaType switch
        {
            "image/gif" => "gif",
            "image/tiff" => "tiff",
            "image/jpeg" => "jpg",
            "image/svg+xml" => "svg",
            "image/png" => "png",
            "image/x-icon" => "ico",
            _ => null,
        };

    public static string GetFilePathWithoutExtension(string filePath)
    {
        var filePathSpan = filePath.AsSpan();
        int length = filePathSpan.LastIndexOf<char>('.');
        return length >= 0 ? filePathSpan.Slice(0, length).ToString() : filePath;
    }

    private static AppPathOptions OrDefault(this AppPathOptions? options) =>
        options ?? new AppPathOptions();
}

public class AppPathOptions
{
    public const string Section = "Paths";

    public string Downloads { get; set; } = "downloads";
}