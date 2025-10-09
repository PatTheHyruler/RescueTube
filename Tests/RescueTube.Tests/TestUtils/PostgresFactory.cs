using Testcontainers.PostgreSql;
using TUnit.Core.Interfaces;

namespace RescueTube.Tests.TestUtils;

public class PostgresFactory : IAsyncInitializer, IAsyncDisposable
{
    private const string Username = "rescue_tube";
    private const string Password = "password123";

    public PostgreSqlContainer PostgreSqlContainer { get; } = new PostgreSqlBuilder()
        .WithUsername(Username)
        .WithPassword(Password)
        .Build();

    public Task InitializeAsync()
    {
        return PostgreSqlContainer.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await PostgreSqlContainer.DisposeAsync();
    }

    public string GetConnectionString(string dbName)
    {
        return $"User ID={Username};Password={Password};Host={PostgreSqlContainer.Hostname};Port={PostgreSqlContainer.GetMappedPublicPort()};Database={dbName}";
    }

    public async Task<string> CreateDatabaseAsync(CancellationToken ct = default)
    {
        var dbName = $"rescue_tube_{Guid.NewGuid()}";
        await PostgreSqlContainer.ExecScriptAsync($"CREATE DATABASE {dbName};", ct);
        var connectionString = GetConnectionString(dbName);
        return connectionString;
    }
}