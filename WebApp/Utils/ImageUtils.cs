using RescueTube.Domain.Entities;

namespace WebApp.Utils;

public static class ImageUtils
{
    public static string GetBaseUrl(this HttpContext context)
    {
        return context.Request.GetBaseUrl();
    }

    public static string GetBaseUrl(this HttpRequest request)
    {
        return $"{request.Scheme}://{request.Host}{request.PathBase}";
    }

    public static string? GetLocalUrl(this Image image, string baseUrl)
    {
        return image.LocalFilePath == null ? null : $"{baseUrl}/{image.LocalFilePath}";
    }

    public static string GetAnyUrl(this Image image, string baseUrl)
    {
        return image.GetLocalUrl(baseUrl) ??
               image.Url ?? throw new Exception($"Failed to create URL for image {image.Id}");
    }
}