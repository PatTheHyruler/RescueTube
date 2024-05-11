using Microsoft.Extensions.Logging;
using RescueTube.Core.Base;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Services;

public class StatusChangeService : BaseService
{
    public StatusChangeService(IServiceProvider services, ILogger logger) : base(services, logger)
    {
    }
    
    public Task Push(StatusChangeEvent statusChangeEvent)
    {
        DbCtx.StatusChangeEvents.Add(statusChangeEvent);
        return Task.CompletedTask; // This method will likely become more complicated and require async later
    }
}