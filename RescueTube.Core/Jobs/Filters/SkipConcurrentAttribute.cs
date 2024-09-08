using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;

namespace RescueTube.Core.Jobs.Filters;

public class SkipConcurrentAttribute : JobFilterAttribute, IServerFilter, IElectStateFilter, IApplyStateFilter
{
    private readonly string _resource;

    public SkipConcurrentAttribute(string resource)
    {
        _resource = resource;
        Order = 50;
    }

    public void OnPerforming(PerformingContext context)
    {
        if (context.BackgroundJob.Job is null) return;
        var storageConnection = context.Connection.AsJobStorageConnection();

        using (AcquireDistributedLock(context.Connection, context.BackgroundJob))
        {
            if (storageConnection.IsJobBlocked(GetResourceKey(context.BackgroundJob),
                    context.BackgroundJob.Id, out _))
            {
                context.Canceled = true;
            }
        }
    }

    public void OnPerformed(PerformedContext context)
    {
    }

    public void OnStateElection(ElectStateContext context)
    {
        if (context.BackgroundJob.Job is null) return;
        var storageConnection = context.Connection.AsJobStorageConnection();

        if (IsStateChanging(context.CandidateState.Name, context.CurrentState,
                EnqueuedState.StateName, ProcessingState.StateName, ScheduledState.StateName))
        {
            using (AcquireDistributedLock(context.Connection, context.BackgroundJob))
            {
                if (storageConnection.IsJobBlocked(GetResourceKey(context.BackgroundJob), context.BackgroundJob.Id,
                        out var blockedBy))
                {
                    context.CandidateState = new DeletedState
                    {
                        Reason = $"Job blocked by {string.Join(", ", blockedBy)}",
                    };
                }
            }
        }
    }

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if (context.BackgroundJob.Job is null) return;
        var storageConnection = context.Connection.AsJobStorageConnection();

        if (IsStateChanging(context.NewState.Name, context.OldStateName,
                EnqueuedState.StateName, ProcessingState.StateName, ScheduledState.StateName))
        {
            using (AcquireDistributedLock(context.Connection, context.BackgroundJob))
            {
                if (storageConnection.IsJobBlocked(GetResourceKey(context.BackgroundJob), context.BackgroundJob.Id,
                        out var blockedBy, out var isBlockedByCurrentJob))
                {
                    throw new Exception($"The job is blocked by {blockedBy}");
                }

                if (!isBlockedByCurrentJob)
                {
                    context.Connection.AddBlock(GetResourceKey(context.BackgroundJob), context.BackgroundJob.Id);
                }
            }
        }
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if (context.BackgroundJob.Job is null) return;
        var storageConnection = context.Connection.AsJobStorageConnection();

        if (IsStateChanging(context.OldStateName, context.NewState.Name,
                EnqueuedState.StateName, ProcessingState.StateName))
        {
            using (AcquireDistributedLock(context.Connection, context.BackgroundJob))
            {
                if (storageConnection.IsJobBlocked(GetResourceKey(context.BackgroundJob), context.BackgroundJob.Id,
                        out var blockedBy, out var isBlockedByCurrentJob))
                {
                    Console.WriteLine(
                        $"The job is unexpectedly blocked by the following jobs when unblocking: {string.Join(", ", blockedBy)}");
                }

                if (isBlockedByCurrentJob)
                {
                    context.Connection.RemoveBlock(GetResourceKey(context.BackgroundJob), context.BackgroundJob.Id);
                }
                else
                {
                    Console.WriteLine($"The job is not blocked by {context.BackgroundJob.Id}, but it should be");
                }
            }
        }
    }

    private static bool IsStateChanging(string stateNameToCheck, string otherStateName,
        params string[] targetStateNames)
    {
        return stateNameToCheck != otherStateName
               && targetStateNames.Contains(stateNameToCheck)
               && !targetStateNames.Contains(otherStateName);
    }

    private IDisposable AcquireDistributedLock(IStorageConnection storageConnection, BackgroundJob backgroundJob,
        TimeSpan? lockWaitTimeout = null)
        => storageConnection.AcquireDistributedLock(
            GetDistributedLockKey(backgroundJob.Job.Args), lockWaitTimeout ?? TimeSpan.FromSeconds(10));

    private string GetDistributedLockKey(IEnumerable<object> args) =>
        $"extension:job-skip-concurrent:lock:{GetKeyFormat(args, _resource)}";

    private string GetResourceKey(BackgroundJob backgroundJob) => GetResourceKey(backgroundJob.Job.Args);

    private string GetResourceKey(IEnumerable<object> args) =>
        $"extension:job-skip-concurrent:set:{GetKeyFormat(args, _resource)}";

    private static string GetKeyFormat(IEnumerable<object> args, string keyFormat)
    {
        return string.Format(keyFormat, args.ToArray());
    }
}