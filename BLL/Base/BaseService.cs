using DAL.EF.DbContexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BLL.Base;

public abstract class BaseService
{
    protected readonly IServiceProvider Services;

    private ServiceUow? _serviceUow;
    public ServiceUow ServiceUow => _serviceUow ??= Services.GetRequiredService<ServiceUow>();

    private AppDbContext? _ctx;
    public AppDbContext Ctx => _ctx ??= Services.GetRequiredService<AppDbContext>();

    protected readonly ILogger Logger;

    protected BaseService(IServiceProvider services, ILogger logger)
    {
        Services = services;
        Logger = logger;
    }
}