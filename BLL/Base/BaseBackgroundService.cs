using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BLL.Base;

public abstract class BaseBackgroundService : BackgroundService
{
    protected readonly IServiceProvider Services;
    protected readonly ILogger Logger;

    protected BaseBackgroundService(IServiceProvider services, ILogger logger)
    {
        Services = services;
        Logger = logger;
    }
}