using Microsoft.Extensions.Options;
using RescueTube.Core.Utils;

namespace RescueTube.YouTube.Services;

public sealed class CookieService
{
    private readonly IOptions<YouTubeOptions> _options;
    private readonly AppPaths _appPaths;

    public CookieService(IOptions<YouTubeOptions> options, AppPaths appPaths)
    {
        _options = options;
        _appPaths = appPaths;
    }

    public string? GetFirstCookieFilePath()
    {
        var cookiesDirectory = _appPaths.GetAbsolutePathFromContentRoot(_options.Value.CookiesDirectory);
        if (!Directory.Exists(cookiesDirectory))
        {
            return null;
        }

        return Directory.EnumerateFiles(cookiesDirectory).FirstOrDefault();
    }
}