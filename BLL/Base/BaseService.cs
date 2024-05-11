using BLL.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BLL.Base;

public abstract class BaseService
{
    protected readonly IServiceProvider Services;

    private ServiceUow? _serviceUow;
    public ServiceUow ServiceUow => _serviceUow ??= Services.GetRequiredService<ServiceUow>();

    public IDataUow DataUow => ServiceUow.DataUow;
    public IAppDbContext DbCtx => DataUow.Ctx;

    protected readonly ILogger Logger;

    protected BaseService(IServiceProvider services, ILogger logger)
    {
        Services = services;
        Logger = logger;
    }
}