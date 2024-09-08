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
        [NotNullWhen(true)] out List<string>? blockedBy
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
        [NotNullWhen(true)] out List<string>? blockedBy,
        out bool isBlockedByCurrentJob
    )
    {
        // Resource set contains a background job id that acquired a mutex for the resource.
        // We are getting only one element to see what background job blocked the invocation.
        blockedBy = jobStorageConnection
            .GetRangeFromSet(setResourceKey, 0, 0);
        isBlockedByCurrentJob = blockedBy?.Contains(backgroundJobId) ?? false;

        if (blockedBy is null or { Count: < 0 })
        {
            return false;
        }

        if (blockedBy.Count == 1 && isBlockedByCurrentJob)
        {
            return false;
        }

        return true;
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