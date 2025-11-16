using Asp.Versioning;
using FluentValidation;
using Hangfire;
using Hangfire.Console;
using Hangfire.Console.Extensions;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using RescueTube.Core;
using RescueTube.Core.Data;
using RescueTube.Core.Identity;
using RescueTube.Core.Jobs;
using RescueTube.Core.Utils;
using RescueTube.DAL.EF.MigrationUtils;
using RescueTube.DAL.EF.Postgres;
using RescueTube.WebApi.ApiModels;
using RescueTube.WebApi.Auth;
using RescueTube.WebApi.Endpoints;
using RescueTube.WebApi.Utils;
using RescueTube.WebApi.Utils.Logging;
using RescueTube.WebApi.Utils.Swagger;
using RescueTube.YouTube;
using Serilog;
using Serilog.Settings.Configuration;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSerilog(c => c
    .ReadFrom.Configuration(builder.Configuration, new ConfigurationReaderOptions { SectionName = "Logging:Serilog" })
    .Enrich.With<ScopePathEnricher>()
);
builder.Services.Configure<HostOptions>(hostOptions =>
{
    hostOptions.BackgroundServiceExceptionBehavior =
        BackgroundServiceExceptionBehavior.Ignore; // Is this a good idea???
});

var useHttpLogging = builder.Configuration.GetValue<bool>("Logging:HTTP:Enabled");
if (useHttpLogging)
{
    builder.Services.AddHttpLogging(logging => { logging.LoggingFields = HttpLoggingFields.All; });
}

builder.Services.AddHttpClient();

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(
        GetHangfireConnectionString(builder))
    )
    .UseConsole()
);
builder.Services.AddHangfireServer(options =>
{
    options.Queues = JobQueues.Queues;
});
builder.Services.AddHangfireConsoleExtensions();
builder.Services.AddSingleton<IDashboardAsyncAuthorizationFilter, HangfireDashboardAuthorizationFilter>();

builder.Services.AddDbPersistenceEfPostgres(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options => JsonUtils.ConfigureJsonSerializerOptions(options.JsonSerializerOptions));
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options => JsonUtils.ConfigureJsonSerializerOptions(options.SerializerOptions));

const string spaDirectory = "ClientApp";
builder.Services.AddSpaStaticFiles(config => { config.RootPath = spaDirectory; });

var apiVersioningBuilder = builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});
apiVersioningBuilder.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen();

const string corsAllowAllName = AuthHelpers.CorsPolicies.CorsAllowAll;
const string corsAllowCredentialsName = AuthHelpers.CorsPolicies.CorsAllowCredentials;
builder.Services.AddCors(options =>
{
    var allowCredentialsOrigins = builder.Configuration
        .GetSection("AllowedCorsCredentialOrigins")
        .Get<string[]>();
    options.AddPolicy(corsAllowCredentialsName, policy =>
    {
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();

        if (allowCredentialsOrigins is { Length: > 0 })
        {
            policy.WithOrigins(allowCredentialsOrigins.ToArray());
        }

        policy.AllowCredentials();
    });
    options.AddPolicy(corsAllowAllName, policy =>
    {
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
        policy.AllowAnyOrigin();
    });
});

builder.AddCustomIdentity<AppDbContext>();
builder.Services
    .AddScoped<HangfireAuthService>()
    .AddScoped<HangfireDashboardAuthenticationMiddleware>();

builder.Services.AddBll();
builder.Services.AddYouTube();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddLocalization();

var app = builder.Build();

// Configure the HTTP request pipeline.

try
{
    if (!app.Environment.IsDevelopment() || true)
    {
        await using var scope = app.Services.CreateAsyncScope();
        await scope.ServiceProvider.MigrateDbAsync<AppDbContext>();
    }

    await app.SeedIdentityAsync();
    await app.SetupYouTubeAsync();

    app.UseHttpsRedirection();

    app.UseWhen(context => context.Request.Path.StartsWithSegments("/api"), apiApp =>
    {
        apiApp.UseExceptionHandler(apiBuilder =>
        {
            apiBuilder.Run(async context =>
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new ErrorResponseDto
                {
                    ErrorType = EErrorType.GenericError,
                    Message = "Something went wrong",
                }, JsonUtils.DefaultJsonSerializerOptions);
            });
        });
    });

    var imagesDirectory = app.Services.GetRequiredService<AppPaths>().GetImagesDirectoryAbsolute();
    var imagesDirectoryPath = Path.Combine(app.Environment.ContentRootPath, imagesDirectory);
    Directory.CreateDirectory(imagesDirectoryPath);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(imagesDirectoryPath),
        RequestPath = "/images",
    });

    app.UseStaticFiles();

    const string hangfirePrefix = "/hangfire-dashboard";
    string[] specialPaths = ["/api", hangfirePrefix];
    bool IsSpecialPath(string path) => specialPaths.Any(path.StartsWith);

    var spaIndexPath = Path.Combine(app.Environment.ContentRootPath, spaDirectory, "index.html");
    if (Path.Exists(spaIndexPath))
    {
        app.MapWhen(c => !IsSpecialPath(c.Request.Path.Value ?? ""),
            spaAppBuilder =>
            {
                spaAppBuilder.UseSpaStaticFiles();
                spaAppBuilder.UseSpa(_ => { });
            });
    }

    app.UseRouting();

    app.UseCors(corsAllowAllName);
    app.UseCors(corsAllowCredentialsName);

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseWhen(context => context.Request.Path.StartsWithSegments(hangfirePrefix), hangfireApp =>
    {
        hangfireApp.UseMiddleware<HangfireDashboardAuthenticationMiddleware>();
        hangfireApp.UseHangfireDashboard(pathMatch: hangfirePrefix, options: new DashboardOptions
        {
            AppPath = null,
            DarkModeEnabled = true,
            Authorization = app.Services.GetRequiredService<IEnumerable<IDashboardAuthorizationFilter>>(),
            AsyncAuthorization = app.Services.GetRequiredService<IEnumerable<IDashboardAsyncAuthorizationFilter>>(),
        });
    });

    var versionedApiBuilder = app.NewVersionedApi().RequireAuthorization();
    var baseVersionedApi = versionedApiBuilder.MapGroup("api/v{version:apiVersion}");

    baseVersionedApi
        .MapGet("/auth/hangfire", (
            [FromServices] HangfireAuthService hangfireAuthService,
            HttpResponse response,
            [FromQuery] string hangfireToken,
            [FromQuery] string targetUrl,
            [FromQuery] string? appAuthUrl = null
        ) => hangfireAuthService.HandleInitialAuth(
            new(HangfireJwt: hangfireToken, TargetUrl: targetUrl, AppAuthUrl: appAuthUrl), response))
        .HasApiVersion(1)
        .AllowAnonymous();
    baseVersionedApi.MapAuthorEndpoints();
    baseVersionedApi.MapAccountEndpoints();
    baseVersionedApi.MapCommentEndpoints();
    baseVersionedApi.MapOptionsEndpoints();
    baseVersionedApi.MapStatisticsEndpoints();
    baseVersionedApi.MapSubmissionEndpoints();
    baseVersionedApi.MapVideoEndpoints();
    baseVersionedApi.MapVideoFileEndpoints();
    baseVersionedApi.MapPlaylistEndpoints();
    baseVersionedApi.MapSettingEndpoints();
    baseVersionedApi.MapDataFetchEndpoints();
    baseVersionedApi.MapJobEndpoints();

    app.MapControllers();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            foreach (var description in app.DescribeApiVersions())
            {
                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName);
            }
        });
    }

    app.Run();
}
catch (Exception e)
{
    try
    {
        await using var scope = app.Services.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(e, "An error occured while running the application");
        await Log.CloseAndFlushAsync();
    }
    catch (Exception logException)
    {
        await Console.Error.WriteLineAsync($"Failed to log catchall application error {logException.GetType()}: {logException.Message}");
    }
    throw;
}

return;

string GetHangfireConnectionString(WebApplicationBuilder webApplicationBuilder)
{
    var s = webApplicationBuilder.Configuration.GetConnectionString("HangfirePostgres");
    if (string.IsNullOrWhiteSpace(s))
    {
        throw new ApplicationException("HangfirePostgres connection string is required");
    }

    return s;
}