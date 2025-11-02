using System.Diagnostics;
using System.Runtime.InteropServices;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RescueTube.Core;
using RescueTube.Core.Contracts;
using RescueTube.Core.DataFetches;
using RescueTube.Core.JobOrchestration;
using RescueTube.Core.Services;
using RescueTube.Core.Utils.Validation;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;
using RescueTube.YouTube.Jobs;
using RescueTube.YouTube.Jobs.DataFetch;
using RescueTube.YouTube.Services;
using RescueTube.YouTube.Services.External;

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

        services.AddMediatR(cfg => { cfg.RegisterServicesFromAssemblyContaining<IYouTubeAssemblyMarker>(); });

        services.AddScoped<IYouTubeDlClient, YouTubeDlClient>();
        services.AddScoped<IYouTubeExplodeClient, YouTubeExplodeClient>();

        services.AddScoped<FetchYouTubeExplodeAuthorDataJob>();
        services.Configure<JobsConfiguration>(c => c.RegisterJobs(
            new JobDefinition<UpdateYtDlpJob>
            {
                IsArchivalJob = true,
                DefaultSettings = new JobSettings
                {
                    JobId = UpdateYtDlpJob.RecurringJobId,
                    Cron = Cron.Daily(),
                    IsEnabled = true,
                },
            },
            new JobDefinition<FetchPlaylistDataJob>
            {
                IsArchivalJob = true,
                DefaultSettings = new JobSettings
                {
                    JobId = FetchPlaylistDataJob.RecurringJobId,
                    Cron = "*/10 * * * *", // Every 10th minute
                    IsEnabled = true,
                },
            },
            new JobDefinition<FetchVideoDataJob>
            {
                IsArchivalJob = true,
                DefaultSettings = new JobSettings
                {
                    JobId = FetchVideoDataJob.RecurringJobId,
                    Cron = "*/10 * * * *", // Every 10th minute
                    IsEnabled = true,
                },
            },
            new JobDefinition<FetchYouTubeExplodeAuthorDataJob>
            {
                IsArchivalJob = true,
                DefaultSettings = new JobSettings
                {
                    JobId = FetchYouTubeExplodeAuthorDataJob.RecurringJobId,
                    Cron = "*/10 * * * *", // Every 10th minute
                    IsEnabled = true,
                },
                Priority = -10,
            },
            new JobDefinition<FetchAuthorVideosJob>
            {
                IsArchivalJob = true,
                DefaultSettings = new JobSettings
                {
                    JobId = FetchAuthorVideosJob.RecurringJobId,
                    Cron = "*/10 * * * *", // Every 10th minute
                    IsEnabled = true,
                },
            }
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
            var ytdl = services.GetRequiredService<IYouTubeDlClient>();
            if (Path.Exists(ytdl.YouTubeDlPath))
            {
                await ytdl.RunUpdateAsync();
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