using System.Security.Cryptography;
using System.Text.Json;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage;

namespace RescueTube.Core.Jobs.Filters;

/// <summary>
/// Skip running the job if the same job with the same arguments is already running
/// </summary>
public class SkipConcurrentSameArgExecutionAttribute : JobFilterAttribute, IServerFilter
{
    private const string JobLockKey = "SameArgJobSkipLock";

    public void OnPerforming(PerformingContext context)
    {
        var job = context.BackgroundJob.Job;
        var key =
            $"{job?.Type?.FullName}-{job?.Method?.Name}-{JsonSerializer.Serialize(job?.Args.Where(o => o is not CancellationToken))}";
        var hash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(key));
        var resource = Convert.ToBase64String(hash);

        try
        {
            var jobLock = context.Storage.GetReadOnlyConnection()
                .AcquireDistributedLock(resource, TimeSpan.FromSeconds(0));
            context.Items[JobLockKey] = jobLock;
        }
        catch (DistributedLockTimeoutException)
        {
            context.Canceled = true;
        }
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