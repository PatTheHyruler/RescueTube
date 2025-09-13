using RescueTube.Core.Data;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Services;

public class StatusChangeService
{
    private readonly AppDbContext _dbCtx;

    public StatusChangeService(AppDbContext dbCtx)
    {
        _dbCtx = dbCtx;
    }

    public void Push(StatusChangeEvent statusChangeEvent)
    {
        // TODO: Notifications
        _dbCtx.StatusChangeEvents.Add(statusChangeEvent);
    }
}