using Microsoft.Extensions.Options;
using RescueTube.Core.Utils;
using RescueTube.Domain.Enums;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;

namespace RescueTube.YouTube.Services.External;

public interface IYouTubeDlClient
{
    string YouTubeDlPath { get; }

    Task<RunResult<VideoData?>?> RunVideoDataFetchAsync(
        string url,
        CancellationToken ct = default,
        bool flat = true,
        bool fetchComments = false,
        OptionSet? overrideOptions = null);

    Task<RunResult<string?>?> RunVideoDownloadAsync(
        string url,
        string format = "bestvideo+bestaudio/best",
        DownloadMergeFormat mergeFormat = DownloadMergeFormat.Unspecified,
        VideoRecodeFormat recodeFormat = VideoRecodeFormat.None,
        CancellationToken ct = default,
        IProgress<DownloadProgress>? progress = null,
        IProgress<string>? output = null,
        OptionSet? overrideOptions = null);

    Task<string?> RunUpdateAsync();
}

public class YouTubeDlClient : IYouTubeDlClient
{
    private readonly YoutubeDL _youtubeDl;
    private readonly CookieService _cookieService;

    public YouTubeDlClient(IOptions<YouTubeOptions> youTubeOptions, AppPaths appPaths, CookieService cookieService)
    {
        _cookieService = cookieService;
        var binariesDirectory = Setup.GetBinariesDirectory(youTubeOptions.Value.BinariesDirectory);
        _youtubeDl = new YoutubeDL
        {
            OutputFolder = appPaths.GetVideosDirectory(EPlatform.YouTube),
            RestrictFilenames = true,
            YoutubeDLPath = Path.Combine(binariesDirectory, YoutubeDLSharp.Utils.YtDlpBinaryName),
            FFmpegPath = Path.Combine(binariesDirectory, YoutubeDLSharp.Utils.FfmpegBinaryName),
            // Can't set ffprobe path??
            OverwriteFiles = false,
        };
    }

    public string YouTubeDlPath => _youtubeDl.YoutubeDLPath;

    private OptionSet? _cachedOptions;

    private async ValueTask<OptionSet> GetOptionsAsync(CancellationToken ct)
    {
        if (_cachedOptions is not null)
        {
            return _cachedOptions;
        }

        var options = new OptionSet();
        await _cookieService.ApplyCookieConfigurationAsync(options, ct);
        _cachedOptions = options;
        return options;
    }

    public async Task<RunResult<VideoData?>?> RunVideoDataFetchAsync(
        string url,
        CancellationToken ct = default,
        bool flat = true,
        bool fetchComments = false,
        OptionSet? overrideOptions = null)
    {
        return await _youtubeDl.RunVideoDataFetch(
            url: url,
            ct: ct,
            flat: flat,
            fetchComments: fetchComments,
            overrideOptions: overrideOptions ?? await GetOptionsAsync(ct));
    }

    public async Task<RunResult<string?>?> RunVideoDownloadAsync(
        string url,
        string format = "bestvideo+bestaudio/best",
        DownloadMergeFormat mergeFormat = DownloadMergeFormat.Unspecified,
        VideoRecodeFormat recodeFormat = VideoRecodeFormat.None,
        CancellationToken ct = default,
        IProgress<DownloadProgress>? progress = null,
        IProgress<string>? output = null,
        OptionSet? overrideOptions = null)
    {
        return await _youtubeDl.RunVideoDownload(
            url: url,
            format: format,
            mergeFormat: mergeFormat,
            recodeFormat: recodeFormat,
            ct: ct,
            progress: progress,
            output: output,
            overrideOptions: overrideOptions ?? await GetOptionsAsync(ct));
    }

    public Task<string?> RunUpdateAsync()
    {
        return _youtubeDl.RunUpdate();
    }
}