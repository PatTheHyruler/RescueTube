using Hangfire;
using Hangfire.Server;
using RescueTube.Core.Identity.Services;
using RescueTube.Core.JobOrchestration;

namespace RescueTube.Core.Jobs;

public class DeleteExpiredRefreshTokensJob : IJob
{
    public static string RecurringJobId => "core:delete-expired-refresh-tokens";

    public static readonly JobDefinition<DeleteExpiredRefreshTokensJob> JobDefinition = new()
    {
        IsArchivalJob = false,
        DefaultSettings = new()
        {
            JobId = RecurringJobId,
            Cron = Cron.Daily(),
            IsEnabled = true,
        },
    };

    private readonly TokenService _tokenService;

    public DeleteExpiredRefreshTokensJob(TokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public Task RunAsync(PerformContext performContext, CancellationToken ct)
    {
        return _tokenService.DeleteExpiredRefreshTokensAsync(ct);
    }
}