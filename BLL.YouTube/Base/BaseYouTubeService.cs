using RescueTube.Core.Base;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BLL.YouTube.Base;

public class BaseYouTubeService : BaseService
{
    public BaseYouTubeService(IServiceProvider services, ILogger<BaseYouTubeService> logger) : base(services, logger)
    {
    }

    private YouTubeUow? _youTubeUow;
    protected YouTubeUow YouTubeUow => _youTubeUow ??= Services.GetRequiredService<YouTubeUow>();
}