using Hangfire;
using YoutubeDLSharp;

namespace RescueTube.YouTube.Jobs;

public class UpdateYtDlpJob
{
    private readonly YoutubeDL _youtubeDl;

    public UpdateYtDlpJob(YoutubeDL youtubeDl)
    {
        _youtubeDl = youtubeDl;
    }

    [DisableConcurrentExecution(60)]
    public async Task UpdateYouTubeDlAsync()
    {
        await _youtubeDl.RunUpdate();
    }
}