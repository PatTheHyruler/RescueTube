using System.Data;
using System.Reflection;
using System.Transactions;
using Dapper;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.Storage;
using IsolationLevel = System.Data.IsolationLevel;

namespace RescueTube.Core.Jobs.Filters;

public static class FilterUtils
{
    public static void TryRemoveLock(string resource, JobStorage jobStorage, IStorageConnection storageConnection)
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