using System.Text.Json;
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
    string? TriggerIfNotRunning(string recurringJobId, bool enqueueContinuationIfRunning = true);
    string[] TriggerIfNotRunning(IEnumerable<string> recurringJobIds, bool enqueueContinuationIfRunning = true);
    Task<JobDefinitionWithSettings[]> GetJobDefinitionsWithSettingsAsync(CancellationToken ct);
    Task<JobSettings?> GetJobSettingsAsync(JobDefinition jobDefinition, CancellationToken ct);
    Task HandleJobSettingsUpdateAsync(IReadOnlyCollection<JobDefinitionWithSettings> updatedSettings,
        CancellationToken ct);
}

public class RecurringJobsService : IRecurringJobsService
{
    private const string CacheKey = "JobSettings";

    private readonly IRecurringJobManagerV2 _recurringJobManager;
    private readonly IBackgroundJobClientV2 _backgroundJobClient;
    private readonly JobsConfiguration _config;
    private readonly SettingService _settingService;
    private readonly IDataUow _dataUow;
    private readonly IMemoryCache _memoryCache;

    public RecurringJobsService(
        IRecurringJobManagerV2 recurringJobManager,
        IBackgroundJobClientV2 backgroundJobClient,
        IOptions<JobsConfiguration> config,
        SettingService settingService,
        IDataUow dataUow,
        IMemoryCache memoryCache)
    {
        _recurringJobManager = recurringJobManager;
        _backgroundJobClient = backgroundJobClient;
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

    public string? TriggerIfNotRunning(string recurringJobId, bool enqueueContinuationIfRunning = true)
    {
        return TriggerIfNotRunning([recurringJobId], enqueueContinuationIfRunning).SingleOrDefault();
    }

    public string[] TriggerIfNotRunning(IEnumerable<string> recurringJobIds, bool enqueueContinuationIfRunning = true)
    {
        using var connection = _recurringJobManager.Storage.GetReadOnlyConnection();
        var jobs = connection.GetRecurringJobs(recurringJobIds)
            .Where(IsRealRecurringJob)
            .ToArray();

        foreach (var recurringJob in jobs)
        {
            if ((recurringJob.LastJobState == EnqueuedState.StateName ||
                recurringJob.LastJobState == ProcessingState.StateName) &&
                !string.IsNullOrEmpty(recurringJob.LastJobId))
            {
                if (enqueueContinuationIfRunning && !HasContinuation(connection, recurringJob))
                {
                    _backgroundJobClient.ContinueJobWith<IRecurringJobManagerV2>(recurringJob.LastJobId, r => r.Trigger(recurringJob.Id));
                }
            }
            else
            {
                _recurringJobManager.TriggerJob(recurringJob.Id);    
            }
        }

        return jobs.Select(j => j.Id).ToArray();
    }

    private static bool HasContinuation(IStorageConnection connection, RecurringJobDto recurringJob)
    {
        var continuations = GetContinuations(connection, recurringJob.LastJobId);
        if (continuations is { Length: > 0 })
        {
            return continuations
                .Select(continuation => connection.GetJobData(continuation.JobId))
                .Any(continuationJobData =>
                    continuationJobData is not null &&
                    continuationJobData.Job.Type == typeof(IRecurringJobManagerV2) &&
                    continuationJobData.Job.Method.Name
                        is nameof(IRecurringJobManagerV2.Trigger)
                        or nameof(IRecurringJobManagerV2.TriggerJob) &&
                    continuationJobData.Job.Args is [string continuationRecurringJobId] &&
                    continuationRecurringJobId == recurringJob.Id);
        }

        return false;
    }

    private static JobContinuationDto[]? GetContinuations(IStorageConnection connection, string jobId)
    {
        var continuationsJson = connection.GetJobParameter(jobId, "Continuations");
        if (continuationsJson is null)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<JobContinuationDto[]>(continuationsJson, HangfireParameterJsonSerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    // ReSharper disable once NotAccessedPositionalProperty.Local
    // ReSharper disable once ClassNeverInstantiated.Local
    private record JobContinuationDto(string JobId, JobContinuationOptions Options);

    private static readonly JsonSerializerOptions HangfireParameterJsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

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