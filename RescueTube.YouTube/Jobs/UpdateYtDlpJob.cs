using RescueTube.Core.Jobs.Filters;
using RescueTube.YouTube.Services.External;

namespace RescueTube.YouTube.Jobs;

public class UpdateYtDlpJob
{
    private readonly IYouTubeDlClient _youtubeDl;

    public UpdateYtDlpJob(IYouTubeDlClient youtubeDl)
    {
        _youtubeDl = youtubeDl;
    }

    [SkipConcurrent("yt:update-youtube-dl")]
    public async Task UpdateYouTubeDlAsync()
    {
        await _youtubeDl.RunUpdateAsync();
    }
}