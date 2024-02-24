using System.Runtime.InteropServices;
using BLL.YouTube.Services;
using Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;
using YoutubeExplode;

namespace BLL.YouTube;

public class YouTubeUow
{
    private readonly IServiceProvider _services;

    public YouTubeUow(IServiceProvider services)
    {
        _services = services;
    }

    private YoutubeDL? _youtubeDl;

    public YoutubeDL YoutubeDl =>
        _youtubeDl ??= new YoutubeDL
        {
            OutputFolder = AppPaths.GetVideosDirectory(EPlatform.YouTube,
                _services.GetService<IOptionsSnapshot<AppPathOptions>>()?.Value),
            RestrictFilenames = true,
            OverwriteFiles = false,
        };

    private YoutubeClient? _youTubeExplodeClient;
    public YoutubeClient YouTubeExplodeClient => _youTubeExplodeClient ??= new YoutubeClient();

    private string GetUniqueFileIdentifier() => $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid()
        .ToString().Replace("-", "")[..8]}";

    private static int UnnecessaryFilePartLimit =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? 20
            : 200;

    public OptionSet DownloadOptions => new()
    {
        WriteInfoJson = true,
        RestrictFilenames = true,
        Output = Path.Combine(
            AppPaths.GetVideosDirectory(EPlatform.YouTube, _services.GetService<AppPathOptions>()),
            $"(%(channel_id)s) %(uploader).{UnnecessaryFilePartLimit}B/%(upload_date)s - %(title).{UnnecessaryFilePartLimit}B - %(id)s/{GetUniqueFileIdentifier()}.%(ext)s"
        ),
    };

    private SubmitService? _submitService;
    public SubmitService SubmitService => _submitService ??= _services.GetRequiredService<SubmitService>();

    private VideoService? _videoService;
    public VideoService VideoService => _videoService ??= _services.GetRequiredService<VideoService>();

    private AuthorService? _authorService;
    public AuthorService AuthorService => _authorService ??= _services.GetRequiredService<AuthorService>();
}