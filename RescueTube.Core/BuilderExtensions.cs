using Microsoft.Extensions.DependencyInjection;
using RescueTube.Core.Contracts;
using RescueTube.Core.Data.Mappers;
using RescueTube.Core.DataFetches;
using RescueTube.Core.JobOrchestration;
using RescueTube.Core.Jobs;
using RescueTube.Core.Jobs.Registration;
using RescueTube.Core.Services;
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

        services.AddSingleton<JobExecutionRegistry>();
        services.AddOptions<JobsConfiguration>();

        services.AddSingleton<ServiceRegistry>();

        services.AddSingleton<DataFetchContext>();

        services.AddScoped<ServiceUow>();

        services.AddScoped<SubmissionService>();
        services.AddScoped<AuthorizationService>();
        services.AddScoped<ImageService>();
        services.AddScoped<VideoPresentationService>();
        services.AddScoped<PlaylistPresentationService>();
        services.AddScoped<EntityUpdateService>();
        services.AddScoped<CommentService>();
        services.AddScoped<StatusChangeService>();
        services.AddScoped<StatisticsPresentationService>();

        services.AddScoped<StorageLimitService>();

        services.AddScoped<EntityMapper>();

        services.AddMediatR(cfg => { cfg.RegisterServicesFromAssemblyContaining<SubmissionService>(); });

        services.AddScoped<SubmissionAddEntityAccessPermissionJob>();
        services.AddScoped<UpdateImagesResolutionJob>();
        services.Configure<JobsConfiguration>(c => c.RegisterJobs(
            new JobDefinition<DownloadImageJob>
            {
                Priority = -1,
                PreferredMaxConcurrentExecutions = 10,
            }
        ));

        services.AddHostedService<RegisterBllJobsService>();

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