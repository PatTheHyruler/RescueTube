using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace RescueTube.Core.Services.Startup;

public class SetupRecurringJobsService : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SetupRecurringJobsService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var recurringJobsService = scope.ServiceProvider.GetRequiredService<RecurringJobsService>();

        await recurringJobsService.SetupRecurringJobsAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}