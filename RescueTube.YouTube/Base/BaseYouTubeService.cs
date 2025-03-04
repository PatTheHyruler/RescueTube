using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Base;
using RescueTube.Core.Contracts;
using RescueTube.Domain.Enums;

namespace RescueTube.YouTube.Base;

public class BaseYouTubeService : BaseService, IPlatformService
{
    public BaseYouTubeService(IServiceProvider services, ILogger<BaseYouTubeService> logger) : base(services, logger)
    {
    }

    private YouTubeUow? _youTubeUow;
    protected YouTubeUow YouTubeUow => _youTubeUow ??= Services.GetRequiredService<YouTubeUow>();

    public EPlatform Platform => EPlatform.YouTube;
}