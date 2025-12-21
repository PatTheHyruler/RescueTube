using Microsoft.Extensions.Options;
using RescueTube.Core.Services;
using RescueTube.Core.Utils;

namespace RescueTube.YouTube.Services;

public sealed class CookieService
{
    private readonly IOptions<YouTubeOptions> _options;
    private readonly AppPaths _appPaths;
    private readonly SettingService _settingService;

    public CookieService(IOptions<YouTubeOptions> options, AppPaths appPaths, SettingService settingService)
    {
        _options = options;
        _appPaths = appPaths;
        _settingService = settingService;
    }

    private string CookiesDirectory => _appPaths.GetAbsolutePathFromContentRoot(_options.Value.CookiesDirectory);
    private const string DefaultCookieFileName = "cookies.txt";

    private string? GetFirstCookieFilePath()
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
        if (!IsFilePathInCookiesDirectory(filePath))
        {
            throw new ArgumentException($"File path '{filePath}' is outside of the cookies directory", nameof(fileName));
        }
        await File.WriteAllTextAsync(filePath, content, ct);
    }

    public bool DeleteCookieFile(string fileName)
    {
        var filePath = Path.Combine(CookiesDirectory, fileName);
        if (!IsFilePathInCookiesDirectory(filePath))
        {
            return false;
        }

        if (!File.Exists(filePath))
        {
            return false;
        }

        File.Delete(filePath);
        return true;
    }

    public bool RenameCookieFile(string oldFileName, string newFileName)
    {
        var oldFilePath = Path.Combine(CookiesDirectory, oldFileName);
        var newFilePath = Path.Combine(CookiesDirectory, newFileName);

        if (!IsFilePathInCookiesDirectory(oldFilePath) || !File.Exists(oldFilePath))
        {
            return false;
        }

        if (!IsFilePathInCookiesDirectory(newFilePath))
        {
            throw new ArgumentException($"File path '{newFilePath}' is outside of the cookies directory", nameof(newFileName));
        }

        File.Move(oldFilePath, newFilePath);
        return true;
    }

    private bool IsFilePathInCookiesDirectory(string filePath)
    {
        return Path.GetDirectoryName(filePath)?.TrimEnd('/') == CookiesDirectory.TrimEnd('/');
    }

    public async Task ApplyCookieConfigurationAsync(YoutubeDLSharp.Options.OptionSet options, CancellationToken ct)
    {
        var shouldUseCookieFile = await _settingService.GetValueAsync(YouTubeSettingDefinitions.UseCookieFile, ct)
                                  ?? YouTubeSettingDefinitions.UseCookieFile.DefaultValue;
        if (!shouldUseCookieFile)
        {
            return;
        }

        var cookieFilePath = GetFirstCookieFilePath();
        if (cookieFilePath is null)
        {
            return;
        }

        options.Cookies = cookieFilePath;
    }
}