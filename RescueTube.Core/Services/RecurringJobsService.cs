using Hangfire;
using Hangfire.States;
using Hangfire.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RescueTube.Core.Constants;
using RescueTube.Core.Data;
using RescueTube.Core.DTO;
using RescueTube.Core.JobOrchestration;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Services;

public interface IRecurringJobsService
{
    Task SetupRecurringJobsAsync(CancellationToken ct);
    Task SetupRecurringJobsAsync(bool disableAllArchival, CancellationToken ct);
    string[] TriggerIfNotRunning(params IEnumerable<string> recurringJobIds);
    Task<JobDefinitionWithSettings[]> GetJobDefinitionsWithSettingsAsync(CancellationToken ct);
    Task<JobSettings?> GetJobSettingsAsync(JobDefinition jobDefinition, CancellationToken ct);
    Task HandleJobSettingsUpdateAsync(IReadOnlyCollection<JobDefinitionWithSettings> updatedSettings,
        CancellationToken ct);
}

public class RecurringJobsService : IRecurringJobsService
{
    private const string CacheKey = "JobSettings";

    private readonly IRecurringJobManagerV2 _recurringJobManager;
    private readonly JobsConfiguration _config;
    private readonly SettingService _settingService;
    private readonly IDataUow _dataUow;
    private readonly IMemoryCache _memoryCache;

    public RecurringJobsService(
        IRecurringJobManagerV2 recurringJobManager,
        IOptions<JobsConfiguration> config,
        SettingService settingService,
        IDataUow dataUow,
        IMemoryCache memoryCache)
    {
        _recurringJobManager = recurringJobManager;
        _settingService = settingService;
        _dataUow = dataUow;
        _memoryCache = memoryCache;
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
        var jobsWithSettings = await GetJobDefinitionsWithSettingsAsync(ct);
        SetupRecurringJobs(jobsWithSettings, disableAllArchival, onlyRemoveSpecifiedJobs: false);
    }

    private void SetupRecurringJobs(
        IReadOnlyCollection<JobDefinitionWithSettings> jobsWithSettings,
        bool disableAllArchival, bool onlyRemoveSpecifiedJobs)
    {
        var enabledJobsWithSettings = jobsWithSettings
            .Where(j => j.JobSettings.IsEnabled)
            .Where(j => !disableAllArchival || !j.JobDefinition.IsArchivalJob)
            .ToArray();

        using var jobStorageConnection = _recurringJobManager.Storage.GetConnection();
        var recurringJobsToRemove = jobStorageConnection
            .GetRecurringJobs()
            .ExceptBy(
                enabledJobsWithSettings
                    .Select(j => j.JobDefinition.JobId),
                r => r.Id);
        if (onlyRemoveSpecifiedJobs)
        {
            recurringJobsToRemove = recurringJobsToRemove.IntersectBy(
                jobsWithSettings.Select(x => x.JobDefinition.JobId),
                r => r.Id);
        }

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

    public string[] TriggerIfNotRunning(params IEnumerable<string> recurringJobIds)
    {
        // TODO: If job is already running, enqueue continuation
        using var connection = _recurringJobManager.Storage.GetReadOnlyConnection();
        var jobs = connection.GetRecurringJobs(recurringJobIds)
            .Where(IsRealRecurringJob)
            .Where(j => j.LastJobState != EnqueuedState.StateName && j.LastJobState != ProcessingState.StateName)
            .ToArray();

        foreach (var recurringJob in jobs)
        {
            _recurringJobManager.TriggerJob(recurringJob.Id);
        }

        return jobs.Select(j => j.Id).ToArray();
    }

    public async Task<JobSettings?> GetJobSettingsAsync(JobDefinition jobDefinition, CancellationToken ct)
    {
        var allJobSettings = await GetJobDefinitionsWithSettingsAsync(ct);
        return allJobSettings.Select(x => x.JobSettings).FirstOrDefault(x => x.JobId == jobDefinition.JobId);
    }

    public async Task<JobDefinitionWithSettings[]> GetJobDefinitionsWithSettingsAsync(CancellationToken ct)
    {
        return await _memoryCache.GetOrCreateAsync(CacheKey, _ => GetJobSettingsWithoutCacheAsync(ct))
            ?? await GetJobSettingsWithoutCacheAsync(ct);
    }

    private async Task<JobDefinitionWithSettings[]> GetJobSettingsWithoutCacheAsync(CancellationToken ct)
    {
        var registeredJobs = _config.RegisteredJobs;
        var jobSettings = await _dataUow.Ctx.JobSettings.ToDictionaryAsync(x => x.JobId, ct);
        var definitionsWithSettings = registeredJobs
            .Select(j => new JobDefinitionWithSettings
            {
                JobDefinition = j,
                JobSettings = jobSettings.GetValueOrDefault(j.JobId) ?? j.DefaultSettings,
            })
            .ToArray();

        return definitionsWithSettings;
    }

    public async Task HandleJobSettingsUpdateAsync(IReadOnlyCollection<JobDefinitionWithSettings> updatedSettings,
        CancellationToken ct)
    {
        var disableAllArchival = await _settingService.GetValueAsync(SettingDefinitions.DisableAllArchival, ct)
                                 ?? SettingDefinitions.DisableAllArchival.DefaultValue;
        _memoryCache.Remove(CacheKey);
        SetupRecurringJobs(updatedSettings, disableAllArchival: disableAllArchival, onlyRemoveSpecifiedJobs: true);
    }
}