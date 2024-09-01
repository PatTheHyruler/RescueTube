using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

namespace RescueTube.Core.Jobs.Filters;

public class RescheduleConcurrentExecutionAttribute : JobFilterAttribute, IElectStateFilter, IApplyStateFilter
{
    public required string Key { get; init; }
    public int RescheduleDelaySeconds { get; init; } = 15;

    public RescheduleConcurrentExecutionAttribute()
    {
        Order = 60;
    }

    private string GetResource(Job job)
    {
        return string.Format("rescheduleconcurrent:" + Key, job.Args.ToArray());
    }

    private const string ConcurrentRescheduleAttemptsKey = "ConcurrentRescheduleAttempts";

    public void OnStateElection(ElectStateContext context)
    {
        if (context.CandidateState.Name == ProcessingState.StateName)
        {
            try
            {
                using var @lock = context.Connection.AcquireDistributedLock(
                    GetResource(context.BackgroundJob.Job), TimeSpan.Zero);
            }
            catch (Exception)
            {
                int? attempts = null;
                try
                {
                    attempts = context.GetJobParameter<int?>(ConcurrentRescheduleAttemptsKey);
                }
                catch (Exception)
                {
                    // ignored
                }

                attempts ??= 0;
                attempts++;
                context.SetJobParameter(ConcurrentRescheduleAttemptsKey, attempts);

                context.CandidateState = new ScheduledState(TimeSpan.FromSeconds(RescheduleDelaySeconds));
            }
        }
    }

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if (context.NewState.Name == ProcessingState.StateName && context.OldStateName != ProcessingState.StateName)
        {
            context.Connection.AcquireDistributedLock(
                GetResource(context.BackgroundJob.Job), TimeSpan.FromSeconds(10));
        }
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if (context.OldStateName == ProcessingState.StateName && context.NewState.Name != context.OldStateName)
        {
            // TODO: Add and use job parameter to avoid releasing on jobs that started processing before filter was applied
            FilterUtils.TryRemoveLock(
                GetResource(context.BackgroundJob.Job), context.Storage, context.Connection);    
        }
    }
}