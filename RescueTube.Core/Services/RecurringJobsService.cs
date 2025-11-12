using Hangfire;
using Hangfire.States;
using Hangfire.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RescueTube.Core.Constants;
using RescueTube.Core.Data;
using RescueTube.Core.JobOrchestration;

namespace RescueTube.Core.Services;

public class RecurringJobsService
{
    private readonly IRecurringJobManagerV2 _recurringJobManager;
    private readonly JobsConfiguration _config;
    private readonly SettingService _settingService;
    private readonly IDataUow _dataUow;

    public RecurringJobsService(
        IRecurringJobManagerV2 recurringJobManager,
        IOptions<JobsConfiguration> config,
        SettingService settingService,
        IDataUow dataUow)
    {
        _recurringJobManager = recurringJobManager;
        _settingService = settingService;
        _dataUow = dataUow;
        _config = config.Value;
    }

    public async Task SetupRecurringJobsAsync(CancellationToken ct)
    {
        var disableAllArchival = await _settingService.GetValueAsync(SettingDefinitions.DisableAllArchival, ct)
                                 ?? SettingDefinitions.DisableAllArchival.DefaultValue;

        await SetupRecurringJobsAsync(disableAllArchival: disableAllArchival, ct);
    }

    public async Task SetupRecurringJobsAsync(bool disableAllArchival, CancellationToken ct)
    {
        var registeredJobs = _config.RegisteredJobs;
        var jobSettings = await _dataUow.Ctx.JobSettings.ToDictionaryAsync(x => x.JobId, ct);
        var enabledJobsWithSettings = registeredJobs
            .Select(j => (
                JobDefinition: j,
                Settings: jobSettings.GetValueOrDefault(j.JobId, j.DefaultSettings)))
            .Where(j => j.Settings.IsEnabled)
            .Where(j => !disableAllArchival || !j.JobDefinition.IsArchivalJob)
            .ToArray();

        using var jobStorageConnection = _recurringJobManager.Storage.GetConnection();
        var recurringJobsToRemove = jobStorageConnection.GetRecurringJobs()
            .ExceptBy(
                enabledJobsWithSettings
                    .Select(j => j.JobDefinition.JobId),
                r => r.Id);
        foreach (var recurringJobToRemove in recurringJobsToRemove)
        {
            _recurringJobManager.RemoveIfExists(recurringJobToRemove.Id);
        }

        foreach (var (jobDefinition, jobSetting) in enabledJobsWithSettings)
        {
            _recurringJobManager.AddOrUpdate(
                jobDefinition.JobId,
                jobDefinition.CreateHangfireJob(),
                jobSetting.Cron,
                jobDefinition.HangfireRecurringJobOptions);
        }
    }

    private static bool IsRealRecurringJob(RecurringJobDto job) =>
        job is { Removed: false, NextExecution: not null } && !string.IsNullOrWhiteSpace(job.Cron);

    public void TriggerIfNotRunning(params IEnumerable<string> recurringJobIds)
    {
        // TODO: If job is already running, enqueue continuation
        using var connection = _recurringJobManager.Storage.GetReadOnlyConnection();
        var jobs = connection.GetRecurringJobs(recurringJobIds)
            .Where(IsRealRecurringJob)
            .Where(j => j.LastJobState != EnqueuedState.StateName && j.LastJobState != ProcessingState.StateName);

        foreach (var recurringJob in jobs)
        {
            _recurringJobManager.TriggerJob(recurringJob.Id);
        }
    }
}