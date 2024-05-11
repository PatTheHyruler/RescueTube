using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Base;

namespace RescueTube.YouTube.Base;

public class BaseYouTubeService : BaseService
{
    public BaseYouTubeService(IServiceProvider services, ILogger<BaseYouTubeService> logger) : base(services, logger)
    {
    }

    private YouTubeUow? _youTubeUow;
    protected YouTubeUow YouTubeUow => _youTubeUow ??= Services.GetRequiredService<YouTubeUow>();
}