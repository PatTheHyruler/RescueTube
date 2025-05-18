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

    public void RemoveAllRecurringJobs()
    {
        using var storageConnection = _recurringJobManager.Storage.GetConnection();
        using var jobLock = AcquireRecurringJobLock(storageConnection);

        var recurringJobs = storageConnection.GetRecurringJobs();
        foreach (var recurringJob in recurringJobs)
        {
            _recurringJobManager.RemoveIfExists(recurringJob.Id);
        }
    }

    public void CreateRecurringJobs()
    {
        using var storageConnection = _recurringJobManager.Storage.GetConnection();
        using var jobLock = AcquireRecurringJobLock(storageConnection);

        foreach (var jobRegistration in _hangfireRecurringJobRegistry.RegisteredJobs)
        {
            jobRegistration.RegistrationAction(_recurringJobManager);
        }
    }

    public void DeleteArchivalRecurringJobs()
    {
        using var storageConnection = _recurringJobManager.Storage.GetConnection();
        using var jobLock = AcquireRecurringJobLock(storageConnection);

        var archivalRecurringJobsIds = _hangfireRecurringJobRegistry.RegisteredJobs
            .Where(x => x.IsArchivalJob)
            .Select(x => x.RecurringJobId);
        var recurringJobs = storageConnection.GetRecurringJobs(archivalRecurringJobsIds);
        foreach (var recurringJob in recurringJobs)
        {
            _recurringJobManager.RemoveIfExists(recurringJob.Id);
        }
    }

    public void CreateArchivalRecurringJobs()
    {
        using var storageConnection = _recurringJobManager.Storage.GetConnection();
        using var jobLock = AcquireRecurringJobLock(storageConnection);

        var archivalRecurringJobs = _hangfireRecurringJobRegistry.RegisteredJobs
            .Where(x => x.IsArchivalJob);
        foreach (var jobRegistration in archivalRecurringJobs)
        {
            jobRegistration.RegistrationAction(_recurringJobManager);
        }
    }

    private static IDisposable? AcquireRecurringJobLock(IStorageConnection connection)
    {
        return connection.AcquireDistributedLock("manage-recurring-jobs", TimeSpan.FromSeconds(10));
    }
}