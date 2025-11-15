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
            DownloadImageJob.JobDefinition,
            EnqueueSubmissionsJob.JobDefinition,
            DownloadVideoJob.JobDefinition,
            DeleteExpiredRefreshTokensJob.JobDefinition,
            UpdateImagesResolutionJob.JobDefinition,
            DataFetchesKillSwitchJob.JobDefinition
        ));

        services.AddScoped<IRecurringJobsService, RecurringJobsService>();
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