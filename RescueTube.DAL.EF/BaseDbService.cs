using Microsoft.Extensions.DependencyInjection;
using RescueTube.Core.Data;

namespace RescueTube.DAL.EF;

public abstract class BaseDbService
{
    private readonly IServiceProvider _services;
    protected AppDbContext Ctx => _services.GetRequiredService<AppDbContext>();
    protected IDataUow DataUow => _services.GetRequiredService<IDataUow>();

    protected BaseDbService(IServiceProvider services)
    {
        _services = services;
    }
}