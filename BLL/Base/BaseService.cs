using DAL.EF.DbContexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BLL.Base;

public abstract class BaseService
{
    protected readonly IServiceProvider Services;

    private ServiceUow? _serviceUow;
    public ServiceUow ServiceUow => _serviceUow ??= Services.GetRequiredService<ServiceUow>();

    private AbstractAppDbContext? _ctx;
    public AbstractAppDbContext Ctx => _ctx ??= Services.GetRequiredService<AbstractAppDbContext>();

    public ILogger Logger => Services.GetRequiredService<ILogger>(); 

    protected BaseService(IServiceProvider services)
    {
        Services = services;
    }
}