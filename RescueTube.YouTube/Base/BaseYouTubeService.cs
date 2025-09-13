using RescueTube.Core.Contracts;
using RescueTube.Domain.Enums;

namespace RescueTube.YouTube.Base;

public class BaseYouTubeService : IPlatformService
{
    public EPlatform Platform => EPlatform.YouTube;
}