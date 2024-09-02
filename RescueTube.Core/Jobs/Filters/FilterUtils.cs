using System.Diagnostics.CodeAnalysis;
using Hangfire.Storage;

namespace RescueTube.Core.Jobs.Filters;

public static class FilterUtils
{
    public static JobStorageConnection AsJobStorageConnection(this IStorageConnection connection)
    {
        if (connection is JobStorageConnection jobStorageConnection)
        {
            return jobStorageConnection;
        }

        throw new NotSupportedException(
            "This version of storage doesn't support extended methods. Please try to update to the latest version.");
    }

    public static bool IsJobBlocked(
        this JobStorageConnection jobStorageConnection,
        string setResourceKey,
        string backgroundJobId
    ) => IsJobBlocked(
        jobStorageConnection,
        setResourceKey,
        backgroundJobId,
        out _
    );

    public static bool IsJobBlocked(
        this JobStorageConnection jobStorageConnection,
        string setResourceKey,
        string backgroundJobId,
        [NotNullWhen(true)] out string? blockedBy
    ) => IsJobBlocked(
        jobStorageConnection,
        setResourceKey,
        backgroundJobId,
        out blockedBy,
        out _
    );

    public static bool IsJobBlocked(
        this JobStorageConnection jobStorageConnection,
        string setResourceKey,
        string backgroundJobId,
        [NotNullWhen(true)] out string? blockedBy,
        out bool isBlockedByCurrentJob
    )
    {
        // Resource set contains a background job id that acquired a mutex for the resource.
        // We are getting only one element to see what background job blocked the invocation.
        var range = jobStorageConnection
            .GetRangeFromSet(setResourceKey, 0, 0);
        blockedBy = range is { Count: > 0 } ? range[0] : null;
        isBlockedByCurrentJob = range?.Contains(backgroundJobId) ?? false;

        // We should permit an invocation only when the set is empty, or if current background
        // job already owns the resource. This may happen when the localTransaction succeeded,
        // but outer transaction failed.
        return blockedBy is not null && blockedBy != backgroundJobId;
    }

    public static void AddBlock(this IStorageConnection connection, string setResourceKey, string backgroundJobId)
    {
        // We need to commit the changes inside a distributed lock, otherwise it's 
        // useless. So we create a local transaction instead of using the 
        // context.Transaction property.
        using var localTransaction = connection.CreateWriteTransaction();
        // Add the current background job identifier to a resource set. This means
        // that resource is owned by the current background job. Identifier will be
        // removed only on failed state, or in one of final states (succeeded or
        // deleted).
        localTransaction.AddToSet(setResourceKey, backgroundJobId);
        localTransaction.Commit();
    }

    public static void RemoveBlock(this IStorageConnection connection, string setResourceKey, string backgroundJobId)
    {
        using var localTransaction = connection.CreateWriteTransaction();
        localTransaction.RemoveFromSet(setResourceKey, backgroundJobId);
        localTransaction.Commit();
    }
}