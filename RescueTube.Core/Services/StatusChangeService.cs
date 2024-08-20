using Microsoft.Extensions.Logging;
using RescueTube.Core.Base;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Services;

public class StatusChangeService : BaseService
{
    public StatusChangeService(IServiceProvider services, ILogger<StatusChangeService> logger) : base(services, logger)
    {
    }

    public void Push(StatusChangeEvent statusChangeEvent)
    {
        // TODO: Notifications
        DbCtx.StatusChangeEvents.Add(statusChangeEvent);
    }
}