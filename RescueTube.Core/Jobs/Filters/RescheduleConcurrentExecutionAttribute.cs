using Hangfire;
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
        Order = 60;
    }

    public int DistributedLockTimeoutSeconds { get; init; } = 10;
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
            context.BackgroundJob.Job is null)
        {
            return;
        }

        // This filter requires an extended set of storage operations. It's supported
        // by all the official storages, and many of the community-based ones.
        var storageConnection = context.Connection.AsJobStorageConnection();

        List<string>? blockedBy;

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
                if (!storageConnection.IsJobBlocked(GetResourceKey(context.BackgroundJob),
                        context.BackgroundJob.Id, out blockedBy))
                {
                    // TODO: Shouldn't we only check in OnStateElection, but actually add the block in OnStateApplied?
                    // The state election could be cancelled by a later filter, surely?
                    context.Connection.AddBlock(GetResourceKey(context.BackgroundJob), context.BackgroundJob.Id);

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
        if (context.BackgroundJob.Job is null) return;

        if (context.OldStateName == ProcessingState.StateName)
        {
            using (AcquireDistributedSetLock(context.Connection, context.BackgroundJob.Job.Args))
            {
                context.Connection.RemoveBlock(GetResourceKey(context.BackgroundJob), context.BackgroundJob.Id);
            }
        }
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
    }

    private static DeletedState CreateDeletedState(List<string> blockedBy)
    {
        return new DeletedState
        {
            Reason = $"Execution was blocked by background job {string.Join(", ", blockedBy)}, all attempts exhausted",
        };
    }

    private ScheduledState CreateScheduledState(List<string> blockedBy, int currentAttempt)
    {
        var reason =
            $"Execution is blocked by background job {string.Join(", ", blockedBy)}, retry attempt: {currentAttempt}";

        if (MaxAttempts > 0)
        {
            reason += $"/{MaxAttempts}";
        }

        return new ScheduledState(TimeSpan.FromSeconds(RetryInSeconds))
        {
            Reason = reason,
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

    private string GetResourceKey(BackgroundJob backgroundJob)
        => GetResourceKey(backgroundJob.Job.Args);

    private string GetResourceKey(IEnumerable<object> args)
    {
        return $"extension:job-reschedule-mutex:set:{GetKeyFormat(args, _resource)}";
    }

    private static string GetKeyFormat(IEnumerable<object> args, string keyFormat)
    {
        return string.Format(keyFormat, args.ToArray());
    }
}