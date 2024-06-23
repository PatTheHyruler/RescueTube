using Microsoft.Extensions.Logging;
using RescueTube.Core.Base;

namespace RescueTube.Core.Services;

public class PlaylistPresentationService : BaseService
{
    public PlaylistPresentationService(IServiceProvider services, ILogger logger) : base(services, logger)
    {
    }
    
    
}