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

    private string CookiesDirectory => _appPaths.GetAbsolutePathFromContentRoot(_options.Value.CookiesDirectory);
    private const string DefaultCookieFileName = "cookies.txt";

    public string? GetFirstCookieFilePath()
    {
        if (!Directory.Exists(CookiesDirectory))
        {
            return null;
        }

        return Directory.EnumerateFiles(CookiesDirectory)
            .Order(StringComparer.InvariantCultureIgnoreCase)
            .FirstOrDefault();
    }

    public IEnumerable<FileInfo> GetCookieFiles()
    {
        if (!Directory.Exists(CookiesDirectory))
        {
            yield break;
        }

        foreach (var filePath in Directory.EnumerateFiles(CookiesDirectory)
                     .Order(StringComparer.InvariantCultureIgnoreCase))
        {
            yield return new FileInfo(filePath);
        }
    }

    public async Task CreateCookieFileAsync(string content, string? fileName, CancellationToken ct)
    {
        Directory.CreateDirectory(CookiesDirectory);
        var filePath = Path.Combine(CookiesDirectory, fileName ?? DefaultCookieFileName);
        await File.WriteAllTextAsync(filePath, content, ct);
    }
}