using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RescueTube.Core.Contracts;
using RescueTube.Core.Utils;
using RescueTube.Core.Utils.Validation;
using RescueTube.Domain.Enums;
using RescueTube.YouTube.EventHandlers;
using RescueTube.YouTube.Jobs;
using RescueTube.YouTube.Jobs.Registration;
using RescueTube.YouTube.Services;
using YoutubeDLSharp;

namespace RescueTube.YouTube;

public static class Setup
{
    private static async Task AddExecutePermission(string filePath)
    {
        var process = new System.Diagnostics.Process();
        const string bashFileName = "/bin/bash";

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && File.Exists(bashFileName))
        {
            process.StartInfo.FileName = bashFileName;
            process.StartInfo.Arguments = $"-c \"chmod +x -- '{filePath}'\"";
            process.StartInfo.UseShellExecute = false;
            process.Start();

            await process.WaitForExitAsync();
        }
    }

    public static void AddYouTube(this IServiceCollection services)
    {
        services.AddOptionsRecursive<YouTubeOptions>(YouTubeOptions.Section);

        services.AddScoped<YouTubeUow>();

        services.AddScoped<SubmitService>();
        services.AddScoped<VideoService>();
        services.AddScoped<PlaylistService>();
        services.AddScoped<AuthorService>();
        services.AddScoped<CommentService>();

        services.AddScoped<IThumbnailComparer, ThumbnailComparer>();

        services.AddScoped<IPlatformSubmissionHandler, SubmitService>();
        services.AddScoped<IPlatformPresentationHandler, PresentationHandler>();

        services.AddMediatR(cfg => { cfg.RegisterServicesFromAssemblyContaining<VideoAddedDownloadHandler>(); });

        services.AddScoped(s =>
        {
            var youTubeOptions = s.GetService<IOptions<YouTubeOptions>>()?.Value;
            var binariesDirectory = GetBinariesDirectory(youTubeOptions?.BinariesDirectory);

            return new YoutubeDL
            {
                OutputFolder = s.GetRequiredService<AppPaths>().GetVideosDirectory(EPlatform.YouTube),
                RestrictFilenames = true,
                YoutubeDLPath = Path.Combine(binariesDirectory, YoutubeDLSharp.Utils.YtDlpBinaryName),
                FFmpegPath = Path.Combine(binariesDirectory, YoutubeDLSharp.Utils.FfmpegBinaryName),
                // Can't set ffprobe path??
                OverwriteFiles = false,
            };
        });

        services.AddScoped<DownloadVideoJob>();
        services.AddScoped<FetchCommentsJob>();
        services.AddScoped<HandleSubmissionJob>();
        services.AddHostedService<RegisterYouTubeJobsService>();
    }

    public static void SetupYouTube(this WebApplication app)
    {
        app.SetupYouTubeAsync().GetAwaiter().GetResult();
    }

    private static async Task SetupYouTubeAsync(this WebApplication app)
    {
        var appBuilder = app as IApplicationBuilder;
        await using var scope = appBuilder.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
            .CreateAsyncScope();
        var services = scope.ServiceProvider;

        var options = services.GetService<IOptions<YouTubeOptions>>()?.Value;
        var binariesDirectory = GetBinariesDirectory(options?.BinariesDirectory);
        var overwriteExistingBinaries = options?.OverwriteExistingBinaries ?? false;
        Directory.CreateDirectory(binariesDirectory);

        await DownloadAndSetupBinaries(binariesDirectory,
            overwriteExistingBinaries);

        if (!overwriteExistingBinaries)
        {
            var ytdl = services.GetRequiredService<YoutubeDL>();
            if (Path.Exists(ytdl.YoutubeDLPath))
            {
                await ytdl.RunUpdate();
            }
        }
    }

    private static string GetBinariesDirectory(string? binariesDirectory)
    {
        if (binariesDirectory != null)
        {
            binariesDirectory = Path.GetFullPath(binariesDirectory);
        }

        binariesDirectory ??= Path.Combine(Directory.GetCurrentDirectory(), "yt-dlp-binaries");
        return binariesDirectory;
    }

    private static async Task DownloadAndSetupBinaries(string binariesDirectory, bool overWriteExistingBinaries = true)
    {
        await YoutubeDLSharp.Utils.DownloadBinaries(skipExisting: !overWriteExistingBinaries,
            directoryPath: binariesDirectory);

        var binaries = new[]
        {
            YoutubeDLSharp.Utils.YtDlpBinaryName,
            YoutubeDLSharp.Utils.FfmpegBinaryName,
            YoutubeDLSharp.Utils.FfprobeBinaryName
        }.Select(b => Path.Combine(binariesDirectory, b));
        foreach (var binary in binaries)
        {
            await AddExecutePermission(binary);
        }
    }
}