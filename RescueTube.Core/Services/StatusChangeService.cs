using Microsoft.Extensions.Logging;
using RescueTube.Core.Base;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Services;

public class StatusChangeService : BaseService
{
    public StatusChangeService(IServiceProvider services, ILogger logger) : base(services, logger)
    {
    }

    public void Push(StatusChangeEvent statusChangeEvent)
    {
        // TODO: Replace with Mediator
        DbCtx.StatusChangeEvents.Add(statusChangeEvent);
    }
}