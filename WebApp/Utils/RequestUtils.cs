using Microsoft.Extensions.Primitives;

namespace WebApp.Utils;

public static class RequestUtils
{
    public static string? GetBaseUrl(this HttpContext context)
    {
        return context.GetBaseUrlFromHeader();
    }

    public static string? GetBaseUrlFromHeader(this HttpContext context)
    {
        var result = context.Request.Headers["X-RescueTube-ApiBaseUrl"];
        return StringValues.IsNullOrEmpty(result) ? null : result[0];
    }

    public static string GetBaseUrlFromPath(this HttpRequest request)
    {
        return $"{request.Scheme}://{request.Host}{request.PathBase}";
    }
}