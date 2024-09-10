using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.Utils;

public class AppPaths
{
    private const string Videos = "videos";
    private const string Images = "images";

    private readonly AppPathOptions _options;
    private readonly string _contentRootPath;

    public AppPaths(IOptions<AppPathOptions> appPathOptions, IWebHostEnvironment environment)
    {
        _contentRootPath = environment.ContentRootPath;
        _options = appPathOptions.Value;
    }

    public string GetVideosDirectory(EPlatform platform) =>
        Path.Combine(_options.Downloads, Videos, platform.ToString());

    public string GetImagesDirectory(EPlatform platform) =>
        Path.Combine(_options.Downloads, Images, platform.ToString());

    public string GetImagesDirectory() => Path.Combine(_options.Downloads, Images);
    public string GetImagesDirectoryAbsolute() => Path.Combine(_contentRootPath, GetImagesDirectory());

    public string GetPathRelativeToDownloads(string path) => Path.GetRelativePath(_options.Downloads, path);

    private string GetPathFromDownloads(string path) =>
        Path.Combine(_options.Downloads, path);

    public string GetAbsolutePathFromDownloads(string path) =>
        GetAbsolutePathFromContentRoot(GetPathFromDownloads(path));

    public string GetAbsolutePathFromContentRoot(string path) => Path.Combine(_contentRootPath, path);
}

public class AppPathOptions
{
    public const string Section = "Paths";

    public string Downloads { get; set; } = "downloads";
}