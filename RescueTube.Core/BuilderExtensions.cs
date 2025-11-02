using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using RescueTube.Core.Constants;
using RescueTube.Core.Contracts;
using RescueTube.Core.Data.Mappers;
using RescueTube.Core.DataFetches;
using RescueTube.Core.JobOrchestration;
using RescueTube.Core.Jobs;
using RescueTube.Core.Services;
using RescueTube.Core.Services.Startup;
using RescueTube.Core.Utils;
using RescueTube.Core.Utils.Validation;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;

namespace RescueTube.Core;

public static class BuilderExtensions
{
    public static IServiceCollection AddBll(this IServiceCollection services)
    {
        services.AddOptionsFull<AppPathOptions>(AppPathOptions.Section);
        services.AddSingleton<AppPaths>();

        services.AddOptions<JobsConfiguration>();

        services.AddOptions<ServiceRegistry>();

        services.AddOptions<DataFetchJobsConfiguration>();
        services.AddScoped<DataFetchService>();

        services.AddScoped<ServiceUow>();

        services.AddScoped<SubmissionService>();
        services.AddScoped<ImageService>();
        services.AddScoped<VideoPresentationService>();
        services.AddScoped<AuthorPresentationService>();
        services.AddScoped<PlaylistPresentationService>();
        services.AddScoped<EntityUpdateService>();
        services.AddScoped<CommentService>();
        services.AddScoped<StatusChangeService>();
        services.AddScoped<StatisticsPresentationService>();

        services.AddScoped<SettingService>();
        services.AddOptions<SettingRegistry>();
        services.Configure<SettingRegistry>(x => x.RegisterDefinitions(SettingDefinitions.AllDefinitions));

        services.AddScoped<StorageLimitService>();

        services.AddScoped<EntityMapper>();

        services.AddMediatR(cfg => { cfg.RegisterServicesFromAssemblyContaining<ICoreAssemblyMarker>(); });

        services.AddScoped<UpdateImagesResolutionJob>();
        services.Configure<JobsConfiguration>(c => c.RegisterJobs(
            new JobDefinition<DownloadImageJob>
            {
                Priority = -1,
                PreferredMaxConcurrentExecutions = 10,
                IsArchivalJob = true,
                DefaultSettings = new JobSettings
                {
                    JobId = DownloadImageJob.RecurringJobId,
                    Cron = Cron.Minutely(),
                    IsEnabled = true,
                },
            },
            new JobDefinition<EnqueueSubmissionsJob>
            {
                IsArchivalJob = true,
                DefaultSettings = new JobSettings
                {
                    JobId = EnqueueSubmissionsJob.RecurringJobId,
                    Cron = Cron.Hourly(),
                    IsEnabled = true,
                },
            },
            new JobDefinition<DownloadVideoJob>
            {
                IsArchivalJob = true,
                DefaultSettings = new JobSettings
                {
                    JobId = DownloadVideoJob.RecurringJobId,
                    Cron = Cron.Minutely(),
                    IsEnabled = true,
                },
            },
            new JobDefinition<DeleteExpiredRefreshTokensJob>
            {
                IsArchivalJob = false,
                DefaultSettings = new JobSettings
                {
                    JobId = DeleteExpiredRefreshTokensJob.RecurringJobId,
                    Cron = Cron.Daily(),
                    IsEnabled = true,
                },
            },
            new JobDefinition<UpdateImagesResolutionJob>
            {
                IsArchivalJob = false,
                DefaultSettings = new JobSettings
                {
                    JobId = UpdateImagesResolutionJob.RecurringJobId,
                    Cron = Cron.Daily(),
                    IsEnabled = true,
                },
            },
            new JobDefinition<DataFetchesKillSwitchJob>
            {
                IsArchivalJob = false,
                DefaultSettings = new JobSettings
                {
                    JobId = DataFetchesKillSwitchJob.RecurringJobId,
                    Cron = "*/10 * * * *", // Every 10th minute
                    IsEnabled = true,
                },
            }
        ));

        services.AddScoped<RecurringJobsService>();
        services.AddHostedService<SetupRecurringJobsService>();

        return services;
    }

    public static IServiceCollection AddPlatformVideoDownloadService<TService>(this IServiceCollection services, EPlatform platform)
        where TService : class, IPlatformVideoDownloadService
    {
        services.AddKeyedScoped<IPlatformVideoDownloadService, TService>(platform);
        services.Configure<ServiceRegistry>(r => r.Register<IPlatformVideoDownloadService>(platform));
        return services;
    }
}