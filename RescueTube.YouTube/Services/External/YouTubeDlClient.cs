using Microsoft.Extensions.Options;
using RescueTube.Core.Utils;
using RescueTube.Domain.Enums;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;

namespace RescueTube.YouTube.Services.External;

public interface IYouTubeDlClient
{
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

    public YouTubeDlClient(IOptions<YouTubeOptions> youTubeOptions, AppPaths appPaths)
    {
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

    public Task<RunResult<VideoData?>?> RunVideoDataFetchAsync(
        string url,
        CancellationToken ct = default,
        bool flat = true,
        bool fetchComments = false,
        OptionSet? overrideOptions = null)
    {
        return _youtubeDl.RunVideoDataFetch(
            url: url,
            ct: ct,
            flat: flat,
            fetchComments: fetchComments,
            overrideOptions: overrideOptions);
    }

    public Task<RunResult<string?>?> RunVideoDownloadAsync(
        string url,
        string format = "bestvideo+bestaudio/best",
        DownloadMergeFormat mergeFormat = DownloadMergeFormat.Unspecified,
        VideoRecodeFormat recodeFormat = VideoRecodeFormat.None,
        CancellationToken ct = default,
        IProgress<DownloadProgress>? progress = null,
        IProgress<string>? output = null,
        OptionSet? overrideOptions = null)
    {
        return _youtubeDl.RunVideoDownload(
            url: url,
            format: format,
            mergeFormat: mergeFormat,
            recodeFormat: recodeFormat,
            ct: ct,
            progress: progress,
            output: output,
            overrideOptions: overrideOptions);
    }

    public Task<string?> RunUpdateAsync()
    {
        return _youtubeDl.RunUpdate();
    }
}