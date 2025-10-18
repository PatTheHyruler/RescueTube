using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using RescueTube.Core.Utils;
using RescueTube.Domain.Enums;
using RescueTube.YouTube.Services;
using RescueTube.YouTube.Services.External;
using YoutubeDLSharp.Options;

namespace RescueTube.YouTube;

public class YouTubeServices
{
    private readonly IServiceProvider _services;

    public YouTubeServices(IServiceProvider services)
    {
        _services = services;
    }

    private IYouTubeDlClient? _youtubeDl;

    public IYouTubeDlClient YoutubeDl =>
        _youtubeDl ??= _services.GetRequiredService<IYouTubeDlClient>();

    private IYouTubeExplodeClient? _youTubeExplodeClient;
    public IYouTubeExplodeClient YouTubeExplodeClient => _youTubeExplodeClient ??= _services.GetRequiredService<IYouTubeExplodeClient>();

    private string GetUniqueFileIdentifier() => $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid()
        .ToString().Replace("-", "")[..8]}";

    private static int UnnecessaryFilePartLimit =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? 20
            : 200;

    public OptionSet CreateDownloadOptions() => new()
    {
        WriteInfoJson = true,
        RestrictFilenames = true,
        Output = Path.Combine(
            _services.GetRequiredService<AppPaths>().GetVideosDirectory(EPlatform.YouTube),
            $"(%(channel_id)s) %(uploader).{UnnecessaryFilePartLimit}B/%(upload_date)s - %(title).{UnnecessaryFilePartLimit}B - %(id)s/{GetUniqueFileIdentifier()}.%(ext)s"
        ),
    };

    private SubmitService? _submitService;
    public SubmitService SubmitService => _submitService ??= _services.GetRequiredService<SubmitService>();

    private VideoService? _videoService;
    public VideoService VideoService => _videoService ??= _services.GetRequiredService<VideoService>();

    private PlaylistService? _playlistService;
    public PlaylistService PlaylistService => _playlistService ??= _services.GetRequiredService<PlaylistService>();

    private AuthorService? _authorService;
    public AuthorService AuthorService => _authorService ??= _services.GetRequiredService<AuthorService>();
}