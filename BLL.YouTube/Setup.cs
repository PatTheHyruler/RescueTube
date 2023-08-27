using System.Runtime.InteropServices;
using BLL.Contracts;
using BLL.YouTube.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Validation;

namespace BLL.YouTube;

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
        services.AddScoped<AuthorService>();

        services.AddScoped<IPlatformSubmissionHandler, SubmitService>();
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
        await DownloadAndSetupBinaries(options?.BinariesDirectory ?? Directory.GetCurrentDirectory(),
            options?.OverwriteExistingBinaries ?? false);
    }

    private static async Task DownloadAndSetupBinaries(string? binariesDirectory = null, bool overWriteExistingBinaries = true)
    {
        if (binariesDirectory != null)
        {
            binariesDirectory = Path.GetFullPath(binariesDirectory);
        }
        binariesDirectory ??= Path.Combine(Directory.GetCurrentDirectory(), "yt-dlp-binaries");
        await YoutubeDLSharp.Utils.DownloadBinaries(skipExisting: !overWriteExistingBinaries, directoryPath: binariesDirectory);

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