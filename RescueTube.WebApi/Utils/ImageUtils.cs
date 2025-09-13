using RescueTube.Domain.Entities;

namespace WebApp.Utils;

public static class ImageUtils
{
    public static string? GetLocalUrl(this Image image, string? baseUrl)
    {
        return image.LocalFilePath == null ? null : $"{baseUrl}/{image.LocalFilePath}";
    }

    public static string? GetAnyUrl(this Image image, string? baseUrl)
    {
        return image.GetLocalUrl(baseUrl) ??
               image.Url;
    }
}