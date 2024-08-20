using System.Security.Cryptography;
using System.Text.Json;
using Hangfire.Common;
using Hangfire.Server;

namespace RescueTube.Core.Jobs.Filters;

/// <summary>
/// If the same job with the same arguments is already running, wait for that job to complete before running the new job
/// </summary>
public class WaitConcurrentSameArgExecutionAttribute : JobFilterAttribute, IServerFilter
{
    private readonly int _timeoutSeconds;

    public WaitConcurrentSameArgExecutionAttribute(int timeoutSeconds)
    {
        if (timeoutSeconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(timeoutSeconds), timeoutSeconds,
                "Timeout must be greater than 0 seconds");
        }

        _timeoutSeconds = timeoutSeconds;
    }

    private const string JobLockKey = "SameArgJobWaitLock";

    public void OnPerforming(PerformingContext context)
    {
        var job = context.BackgroundJob.Job;
        var key =
            $"{job?.Type?.FullName}-{job?.Method?.Name}-{JsonSerializer.Serialize(job?.Args.Where(o => o is not CancellationToken))}";
        var hash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(key));
        var resource = Convert.ToBase64String(hash);

        var jobLock = context.Storage.GetReadOnlyConnection()
            .AcquireDistributedLock(resource, TimeSpan.FromSeconds(_timeoutSeconds));
        context.Items[JobLockKey] = jobLock;
    }

    public void OnPerformed(PerformedContext context)
    {
        if (!context.Items.TryGetValue(JobLockKey, out var lockValue) || lockValue is not IDisposable jobLock)
        {
            return;
        }

        jobLock.Dispose();
    }
}