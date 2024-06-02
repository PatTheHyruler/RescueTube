using RescueTube.Core.Utils.Validation;
using Microsoft.Extensions.DependencyInjection;
using RescueTube.Core.Jobs;
using RescueTube.Core.Jobs.Registration;
using RescueTube.Core.Services;

namespace RescueTube.Core;

public static class BuilderExtensions
{
    public static IServiceCollection AddBll(this IServiceCollection services)
    {
        services.AddOptionsFull<AppPathOptions>(AppPathOptions.Section);

        services.AddScoped<ServiceUow>();

        services.AddScoped<SubmissionService>();
        services.AddScoped<AuthorizationService>();
        services.AddScoped<ImageService>();
        services.AddScoped<VideoPresentationService>();
        services.AddScoped<EntityUpdateService>();
        services.AddScoped<AuthorService>();
        services.AddScoped<VideoService>();
        services.AddScoped<CommentService>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<SubmissionService>();
        });

        services.AddScoped<DownloadAuthorImagesJob>();
        services.AddScoped<DownloadVideoImagesJob>();
        services.AddScoped<DownloadImageJob>();

        services.AddHostedService<RegisterBllJobsService>();

        services.AddAutoMapper(typeof(DTO.AutoMapperConfig));

        return services;
    }
}