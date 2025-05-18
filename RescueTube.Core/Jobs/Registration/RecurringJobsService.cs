using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Options;

namespace RescueTube.Core.Jobs.Registration;

public class RecurringJobsService
{
    private readonly IRecurringJobManagerV2 _recurringJobManager;
    private readonly HangfireRecurringJobRegistry _hangfireRecurringJobRegistry;

    public RecurringJobsService(
        IRecurringJobManagerV2 recurringJobManager,
        IOptions<HangfireRecurringJobRegistry> hangfireRecurringJobRegistry)
    {
        _recurringJobManager = recurringJobManager;
        _hangfireRecurringJobRegistry = hangfireRecurringJobRegistry.Value;
    }

    public void RegisterRecurringJobs()
    {
        using var storageConnection = _recurringJobManager.Storage.GetConnection();
        using var jobLock = storageConnection.AcquireDistributedLock("manage-recurring-jobs", TimeSpan.FromSeconds(10));

        var recurringJobs = storageConnection.GetRecurringJobs();
        foreach (var recurringJob in recurringJobs)
        {
            _recurringJobManager.RemoveIfExists(recurringJob.Id);
        }

        foreach (var jobRegistration in _hangfireRecurringJobRegistry.RegisteredJobs)
        {
            jobRegistration.RegistrationAction(_recurringJobManager);
        }
    }
}