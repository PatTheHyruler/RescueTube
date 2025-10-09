using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RescueTube.Core;
using RescueTube.Core.Contracts;
using RescueTube.Core.DataFetches;
using RescueTube.Core.JobOrchestration;
using RescueTube.Core.Services;
using RescueTube.Core.Utils.Validation;
using RescueTube.Domain.Enums;
using RescueTube.YouTube.EventHandlers;
using RescueTube.YouTube.Jobs;
using RescueTube.YouTube.Jobs.DataFetch;
using RescueTube.YouTube.Services;
using RescueTube.YouTube.Services.External;
using YoutubeDLSharp;

namespace RescueTube.YouTube;

public static class Setup
{
    private static async Task AddExecutePermission(string filePath)
    {
        var process = new Process();
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

        services.AddScoped<YouTubeServices>();

        services.AddScoped<SubmitService>();
        services.AddScoped<VideoService>();
        services.AddPlatformVideoDownloadService<VideoDownloadService>(EPlatform.YouTube);
        services.AddScoped<PlaylistService>();
        services.AddScoped<AuthorService>();
        services.AddScoped<CookieService>();

        services.AddScoped<IThumbnailComparer, ThumbnailComparer>();

        services.AddScoped<IPlatformSubmissionHandler, SubmitService>();
        services.AddScoped<IPlatformPresentationHandler, PresentationHandler>();

        services.AddMediatR(cfg => { cfg.RegisterServicesFromAssemblyContaining<VideoAddedCommentFetchHandler>(); });

        services.AddScoped<IYouTubeDlClient, YouTubeDlClient>();
        services.AddScoped<IYouTubeExplodeClient, YouTubeExplodeClient>();

        services.AddScoped<FetchYouTubeExplodeAuthorDataJob>();
        services.RegisterYouTubeRecurringJobs();

        services.Configure<JobsConfiguration>(c => c.RegisterJobs(
            new JobDefinition<FetchPlaylistDataJob>(),
            new JobDefinition<FetchVideoDataJob>(),
            new JobDefinition<FetchYouTubeExplodeAuthorDataJob>
            {
                Priority = -10,
            },
            new JobDefinition<FetchAuthorVideosJob>()
        ));

        services.Configure<DataFetchJobsConfiguration>(c => c
            .RegisterJob<FetchPlaylistDataJob>()
            .RegisterJob<FetchVideoDataJob>()
            .RegisterJob<FetchYouTubeExplodeAuthorDataJob>()
            .RegisterJob<FetchAuthorVideosJob>());

        services.Configure<SettingRegistry>(x => x.RegisterDefinitions(YouTubeSettingDefinitions.AllDefinitions));
    }

    public static async Task SetupYouTubeAsync(this WebApplication app)
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

    public static string GetBinariesDirectory(string? binariesDirectory)
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