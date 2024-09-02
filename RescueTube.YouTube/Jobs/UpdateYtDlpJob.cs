using RescueTube.Core.Jobs.Filters;
using YoutubeDLSharp;

namespace RescueTube.YouTube.Jobs;

public class UpdateYtDlpJob
{
    private readonly YoutubeDL _youtubeDl;

    public UpdateYtDlpJob(YoutubeDL youtubeDl)
    {
        _youtubeDl = youtubeDl;
    }

    [SkipConcurrent("yt:update-youtube-dl")]
    public async Task UpdateYouTubeDlAsync()
    {
        await _youtubeDl.RunUpdate();
    }
}