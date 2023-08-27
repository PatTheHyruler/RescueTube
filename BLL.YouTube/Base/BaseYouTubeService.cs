using BLL.Base;
using Microsoft.Extensions.DependencyInjection;

namespace BLL.YouTube.Base;

public class BaseYouTubeService : BaseService
{
    public BaseYouTubeService(IServiceProvider services) : base(services)
    {
    }

    private YouTubeUow? _youTubeUow;
    protected YouTubeUow YouTubeUow => _youTubeUow ??= Services.GetRequiredService<YouTubeUow>();
}