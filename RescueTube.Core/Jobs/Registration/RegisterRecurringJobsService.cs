using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace RescueTube.Core.Jobs.Registration;

public class RegisterRecurringJobsService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public RegisterRecurringJobsService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var recurringJobsService = scope.ServiceProvider.GetRequiredService<RecurringJobsService>();

        recurringJobsService.RegisterRecurringJobs();
    }
}