using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Options;
using RescueTube.Core.Constants;
using RescueTube.Core.Services;

namespace RescueTube.Core.Jobs.Registration;

public class RecurringJobsService
{
    private readonly IRecurringJobManagerV2 _recurringJobManager;
    private readonly HangfireRecurringJobRegistry _hangfireRecurringJobRegistry;
    private readonly SettingService _settingService;

    public RecurringJobsService(
        IRecurringJobManagerV2 recurringJobManager,
        IOptions<HangfireRecurringJobRegistry> hangfireRecurringJobRegistry,
        SettingService settingService)
    {
        _recurringJobManager = recurringJobManager;
        _settingService = settingService;
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

    public async Task CreateRecurringJobsAsync(CancellationToken ct)
    {
        using var storageConnection = _recurringJobManager.Storage.GetConnection();
        using var jobLock = AcquireRecurringJobLock(storageConnection);

        var disableAllArchival = await _settingService.GetValueAsync(SettingDefinitions.DisableAllArchival, ct)
                                 ?? SettingDefinitions.DisableAllArchival.DefaultValue;

        var jobRegistrations = disableAllArchival
            ? _hangfireRecurringJobRegistry.RegisteredJobs.Where(x => !x.IsArchivalJob)
            : _hangfireRecurringJobRegistry.RegisteredJobs;
        foreach (var jobRegistration in jobRegistrations)
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