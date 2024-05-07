using Hangfire;
using YoutubeDLSharp;

namespace BLL.YouTube.Jobs;

public class UpdateYtDlpJob
{
    private readonly YoutubeDL _youtubeDl;

    public UpdateYtDlpJob(YoutubeDL youtubeDl)
    {
        _youtubeDl = youtubeDl;
    }

    [DisableConcurrentExecution(60)]
    public async Task UpdateYouTubeDl()
    {
        await _youtubeDl.RunUpdate();
    }
}