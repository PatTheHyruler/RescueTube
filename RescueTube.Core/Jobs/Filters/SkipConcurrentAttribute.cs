using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;

namespace RescueTube.Core.Jobs.Filters;

/// <summary>
/// Skip enqueueing/running the job if the same job with the same arguments is already enqueued/running
/// </summary>
public class SkipConcurrentAttribute : JobFilterAttribute, IServerFilter, IElectStateFilter, IApplyStateFilter
{
    /// <summary>
    /// Unique key to lock the job with.
    /// Can reference job arguments by index using format identifiers like {0}
    /// </summary>
    public required string Key { get; init; }

    public SkipConcurrentAttribute()
    {
        Order = 50;
    }

    private const string LockItemKey = "skipconcurrent_distributedlock";

    private string GetResource(Job job)
    {
        return string.Format("skipconcurrent:" + Key, job.Args.ToArray());
    }

    public void OnPerforming(PerformingContext context)
    {
        IDisposable distributedLock;
        try
        {
            distributedLock = AcquireDistributedLock(context.Connection, context.BackgroundJob);
        }
        catch (Exception)
        {
            context.Canceled = true;
            return;
        }

        context.Items[LockItemKey] = distributedLock;
    }

    public void OnPerformed(PerformedContext context)
    {
        if (context.Items[LockItemKey] is IDisposable distributedLock)
        {
            distributedLock.Dispose();
        }
        else
        {
            TryRemoveLock(context.BackgroundJob.Job, context.Storage, context.Connection);
        }
    }

    public void OnStateElection(ElectStateContext context)
    {
        if (context.CandidateState.Name == EnqueuedState.StateName && context.CurrentState != EnqueuedState.StateName)
        {
            try
            {
                using var @lock = AcquireDistributedLock(context.Connection, context.BackgroundJob);
            }
            catch (Exception e)
            {
                context.CandidateState = new DeletedState(new ExceptionInfo(e));
            }
        }
    }

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if (context.NewState.Name == EnqueuedState.StateName && context.NewState.Name != context.OldStateName)
        {
            AcquireDistributedLock(context.Connection, context.BackgroundJob);
        }
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if (context.OldStateName == EnqueuedState.StateName && context.NewState.Name != context.OldStateName)
        {
            TryRemoveLock(context.BackgroundJob.Job, context.Storage, context.Connection);
        }
    }

    private IDisposable AcquireDistributedLock(IStorageConnection storageConnection, BackgroundJob backgroundJob)
        => storageConnection.AcquireDistributedLock(GetResource(backgroundJob.Job), TimeSpan.Zero);

    private void TryRemoveLock(Job job, JobStorage jobStorage, IStorageConnection storageConnection)
        => FilterUtils.TryRemoveLock(GetResource(job), jobStorage, storageConnection);
}