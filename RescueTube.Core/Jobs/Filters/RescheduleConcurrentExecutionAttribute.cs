using Hangfire.Common;
using Hangfire.PostgreSql;
using Hangfire.States;
using Hangfire.Storage;

namespace RescueTube.Core.Jobs.Filters;

/// <summary>
/// Represents a background job filter that helps to disable concurrent execution
/// without causing worker to wait as in <see cref="Hangfire.DisableConcurrentExecutionAttribute"/>.
/// <br/>
/// Essentially the Mutex filter from Hangfire.Pro,
/// copied from <see href="https://gist.github.com/odinserj/4a3bf40606c4da9183588a5a325dfb99"/> and slightly modified.
/// <br/>
/// This is better than <see cref="Hangfire.DisableConcurrentExecutionAttribute"/> for at least 2 reasons:<br/>
/// 1) It doesn't block the "Processing" jobs with the same job, waiting for a lock to be released<br/>
/// 2) It doesn't break everything when using Hangfire.Console and ambient transactions,
/// because it doesn't hold a transaction for the duration of execution.
/// See also <see href="https://github.com/pieceofsummer/Hangfire.Console/issues/50"/>
/// </summary>
public class RescheduleConcurrentExecutionAttribute : JobFilterAttribute, IElectStateFilter, IApplyStateFilter
{
    private const string MutexRescheduleAttempt = "MutexRescheduleAttempt";
    private readonly string _resource;

    public RescheduleConcurrentExecutionAttribute(string resource)
    {
        _resource = resource;
    }

    public int DistributedLockTimeoutSeconds { get; init; } = 5;
    public int RetryInSeconds { get; init; } = 15;
    public int? MaxAttempts { get; init; }
    private TimeSpan DistributedLockTimeout => TimeSpan.FromMinutes(DistributedLockTimeoutSeconds);

    public void OnStateElection(ElectStateContext context)
    {
        // We are intercepting transitions to the Processed state, that is performed by
        // a worker just before processing a job. During the state election phase we can
        // change the target state to another one, causing a worker not to process the
        // background job.
        if (context.CandidateState.Name != ProcessingState.StateName ||
            context.BackgroundJob.Job == null)
        {
            return;
        }

        // This filter requires an extended set of storage operations. It's supported
        // by all the official storages, and many of the community-based ones.
        if (context.Connection is not JobStorageConnection storageConnection)
        {
            throw new NotSupportedException(
                "This version of storage doesn't support extended methods. Please try to update to the latest version.");
        }

        string? blockedBy;

        try
        {
            // Distributed lock is needed here only to prevent a race condition, when another 
            // worker picks up a background job with the same resource between GET and SET 
            // operations.
            // There will be no race condition, when two or more workers pick up background job
            // with the same id, because state transitions are protected with distributed lock
            // themselves.
            using (AcquireDistributedSetLock(context.Connection, context.BackgroundJob.Job.Args))
            {
                // Resource set contains a background job id that acquired a mutex for the resource.
                // We are getting only one element to see what background job blocked the invocation.
                var range = storageConnection.GetRangeFromSet(
                    GetResourceKey(context.BackgroundJob.Job.Args),
                    0,
                    0);

                blockedBy = range.Count > 0 ? range[0] : null;

                // We should permit an invocation only when the set is empty, or if current background
                // job already owns the resource. This may happen when the localTransaction succeeded,
                // but outer transaction failed.
                if (blockedBy == null || blockedBy == context.BackgroundJob.Id)
                {
                    // We need to commit the changes inside a distributed lock, otherwise it's 
                    // useless. So we create a local transaction instead of using the 
                    // context.Transaction property.
                    var localTransaction = context.Connection.CreateWriteTransaction();

                    // Add the current background job identifier to a resource set. This means
                    // that resource is owned by the current background job. Identifier will be
                    // removed only on failed state, or in one of final states (succeeded or
                    // deleted).
                    localTransaction.AddToSet(GetResourceKey(context.BackgroundJob.Job.Args), context.BackgroundJob.Id);
                    localTransaction.Commit();

                    // Invocation is permitted, and we did all the required things.
                    return;
                }
            }
        }
        catch (Exception e)
        {
            if (e is not (DistributedLockTimeoutException or PostgreSqlDistributedLockException))
            {
                throw;
            }
            // We weren't able to acquire a distributed lock within a specified window. This may
            // be caused by network delays, storage outages or abandoned locks in some storages.
            // Since it is required to expire abandoned locks after some time, we can simply
            // postpone the invocation.
            context.CandidateState = new ScheduledState(TimeSpan.FromSeconds(RetryInSeconds))
            {
                Reason = "Couldn't acquire a distributed lock for job reschedule mutex: timeout exceeded"
            };

            return;
        }

        // Background job execution is blocked. We should change the target state either to 
        // the Scheduled or to the Deleted one, depending on current retry attempt number.
        var currentAttempt = context.GetJobParameter<int>(MutexRescheduleAttempt) + 1;
        context.SetJobParameter(MutexRescheduleAttempt, currentAttempt);

        context.CandidateState = MaxAttempts is null || currentAttempt <= MaxAttempts
            ? CreateScheduledState(blockedBy, currentAttempt)
            : CreateDeletedState(blockedBy);
    }

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if (context.BackgroundJob.Job == null) return;

        if (context.OldStateName == ProcessingState.StateName)
        {
            using (AcquireDistributedSetLock(context.Connection, context.BackgroundJob.Job.Args))
            {
                var localTransaction = context.Connection.CreateWriteTransaction();
                localTransaction.RemoveFromSet(GetResourceKey(context.BackgroundJob.Job.Args),
                    context.BackgroundJob.Id);

                localTransaction.Commit();
            }
        }
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
    }

    private static DeletedState CreateDeletedState(string blockedBy)
    {
        return new DeletedState
        {
            Reason = $"Execution was blocked by background job {blockedBy}, all attempts exhausted"
        };
    }

    private ScheduledState CreateScheduledState(string blockedBy, int currentAttempt)
    {
        var reason = $"Execution is blocked by background job {blockedBy}, retry attempt: {currentAttempt}";

        if (MaxAttempts > 0)
        {
            reason += $"/{MaxAttempts}";
        }

        return new ScheduledState(TimeSpan.FromSeconds(RetryInSeconds))
        {
            Reason = reason
        };
    }

    private IDisposable AcquireDistributedSetLock(IStorageConnection connection, IEnumerable<object> args)
    {
        return connection.AcquireDistributedLock(GetDistributedLockKey(args), DistributedLockTimeout);
    }

    private string GetDistributedLockKey(IEnumerable<object> args)
    {
        return $"extension:job-reschedule-mutex:lock:{GetKeyFormat(args, _resource)}";
    }

    private string GetResourceKey(IEnumerable<object> args)
    {
        return $"extension:job-reschedule-mutex:set:{GetKeyFormat(args, _resource)}";
    }

    private static string GetKeyFormat(IEnumerable<object> args, string keyFormat)
    {
        return string.Format(keyFormat, args.ToArray());
    }
}