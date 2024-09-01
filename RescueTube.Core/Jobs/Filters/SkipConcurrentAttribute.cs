using System.Data;
using System.Reflection;
using System.Transactions;
using Dapper;
using Hangfire;
using Hangfire.Common;
using Hangfire.PostgreSql;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;
using IsolationLevel = System.Data.IsolationLevel;

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
        if (context.NewState.Name == EnqueuedState.StateName)
        {
            AcquireDistributedLock(context.Connection, context.BackgroundJob);
        }
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if (context.OldStateName == EnqueuedState.StateName)
        {
            TryRemoveLock(context.BackgroundJob.Job, context.Storage, context.Connection);
        }
    }

    private IDisposable AcquireDistributedLock(IStorageConnection storageConnection, BackgroundJob backgroundJob)
        => storageConnection.AcquireDistributedLock(GetResource(backgroundJob.Job), TimeSpan.Zero);

    private void TryRemoveLock(Job job, JobStorage jobStorage, IStorageConnection storageConnection)
        => TryRemoveLock(GetResource(job), jobStorage, storageConnection);

    private static void TryRemoveLock(string resource, JobStorage jobStorage, IStorageConnection storageConnection)
    {
        if (storageConnection is not PostgreSqlConnection postgreSqlConnection)
        {
            throw new ArgumentException("Expected Postgres connection");
        }

        var storageCreateAndOpenConnectionMethod = typeof(PostgreSqlStorage).GetMethod("CreateAndOpenConnection",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var storageReleaseConnectionMethod =
            typeof(PostgreSqlStorage).GetMethod("ReleaseConnection", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new Exception($"Failed to get method {nameof(PostgreSqlStorage)}.ReleaseConnection");

        var dbConnectionField =
            typeof(PostgreSqlConnection).GetField("_dedicatedConnection",
                BindingFlags.NonPublic | BindingFlags.Instance);
        var dedicatedConnection = dbConnectionField?.GetValue(postgreSqlConnection) as IDbConnection;
        IDbConnection? newConnection = null;
        IDbTransaction? transaction = null;
        try
        {
            if (dedicatedConnection is null)
            {
                newConnection = storageCreateAndOpenConnectionMethod?.Invoke(jobStorage, []) as IDbConnection;
            }

            var connection = dedicatedConnection ?? newConnection
                ?? throw new Exception($"Failed to get {nameof(IDbConnection)}");
            transaction = Transaction.Current == null
                ? connection.BeginTransaction(IsolationLevel.ReadCommitted)
                : null;

            var options = typeof(PostgreSqlConnection)
                              .GetField("_options", BindingFlags.NonPublic | BindingFlags.Instance)?
                              .GetValue(postgreSqlConnection) as PostgreSqlStorageOptions
                          ?? throw new Exception($"Failed to get {nameof(PostgreSqlStorageOptions)}");
            var actualResource = $"{options.SchemaName}:{resource}";

            connection.Execute(
                $"""DELETE FROM "{options.SchemaName}"."lock" WHERE "resource" = @Resource""",
                new
                {
                    Resource = actualResource,
                }, transaction);
            transaction?.Commit();
        }
        finally
        {
            if (newConnection is not null)
            {
                storageReleaseConnectionMethod.Invoke(jobStorage, [newConnection]);
            }

            transaction?.Dispose();
        }
    }
}