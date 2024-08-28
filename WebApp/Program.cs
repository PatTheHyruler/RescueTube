using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using RescueTube.Core;
using RescueTube.Core.Identity;
using Hangfire;
using Hangfire.Console;
using Hangfire.Console.Extensions;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using RescueTube.Core.Data;
using RescueTube.Core.Jobs;
using RescueTube.Core.Utils;
using RescueTube.DAL.EF.MigrationUtils;
using RescueTube.DAL.EF.Postgres;
using RescueTube.YouTube;
using Serilog;
using Serilog.Settings.Configuration;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebApp.ApiModels;
using WebApp.Auth;
using WebApp.Utils;
using WebApp.Utils.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Logging.ClearProviders();
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration, new ConfigurationReaderOptions { SectionName = "Logging:Serilog" })
    .Enrich.With<ScopePathEnricher>()
    .CreateLogger();
builder.Logging.AddSerilog();
builder.Services.AddSingleton(Log.Logger);
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
    .UseSerilogLogProvider()
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
builder.Services.AddBll();
builder.Services.AddYouTube();

builder.Services.AddLocalization();

var app = builder.Build();

// Configure the HTTP request pipeline.

if (!app.Environment.IsDevelopment() || true)
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.MigrateDbAsync<AppDbContext>();
}

app.SeedIdentity();
app.SetupYouTube();

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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName);
        }
    });
}

var imagesDirectory = app.Services.GetRequiredService<AppPaths>().GetImagesDirectoryAbsolute();
var imagesDirectoryPath = Path.Combine(app.Environment.ContentRootPath, imagesDirectory);
Directory.CreateDirectory(imagesDirectoryPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imagesDirectoryPath),
    RequestPath = "/images",
});

app.UseStaticFiles();

string[] specialPaths = ["/api", "/hangfire"];
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

app.UseHangfireDashboard(options: new DashboardOptions
{
    AppPath = null,
    DarkModeEnabled = true,
    Authorization = app.Services.GetRequiredService<IEnumerable<IDashboardAuthorizationFilter>>(),
    AsyncAuthorization = app.Services.GetRequiredService<IEnumerable<IDashboardAsyncAuthorizationFilter>>(),
});

app.MapControllers();

app.Run();
return;

string GetHangfireConnectionString(WebApplicationBuilder webApplicationBuilder)
{
    var s = webApplicationBuilder.Configuration.GetConnectionString("HangfirePostgres");
    if (string.IsNullOrWhiteSpace(s))
    {
        throw new ApplicationException("HangfirePostgres connection string is required");
    }

    if (!s.Contains("Enlist=true"))
    {
        if (!s.EndsWith(';'))
        {
            s += ';';
        }

        s += "Enlist=true";
    }

    return s;
}