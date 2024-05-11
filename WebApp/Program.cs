using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using RescueTube.Core;
using RescueTube.Core.Identity;
using BLL.YouTube;
using DAL.EF;
using DAL.EF.Postgres;
using Hangfire;
using Hangfire.Console;
using Hangfire.Console.Extensions;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Settings.Configuration;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebApp.ApiModels;
using WebApp.Auth;
using WebApp.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Logging.ClearProviders();
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration, new ConfigurationReaderOptions { SectionName = "Logging:Serilog" })
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
        builder.Configuration.GetConnectionString("HangfirePostgres"))
    )
    .UseSerilogLogProvider()
    .UseConsole()
);
builder.Services.AddHangfireServer();
builder.Services.AddHangfireConsoleExtensions();

builder.Services.AddDbPersistenceEfPostgres(builder.Configuration);

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options => JsonUtils.ConfigureJsonSerializerOptions(options.JsonSerializerOptions));
builder.Services.AddMvc();

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

const string corsAllowAllName = "CorsAllowAll";
const string corsAllowCredentialsName = "CorsAllowCredentials";
builder.Services.AddCors(options =>
{
    var allowCredentialsOrigins = builder.Configuration.GetValue<List<string>?>("AllowedCorsCredentialOrigins");
    options.AddPolicy(corsAllowCredentialsName, policy =>
    {
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();

        if (allowCredentialsOrigins is { Count: > 0 })
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

app.UseStaticFiles();
var imagesDirectory = AppPaths.GetImagesDirectory(AppPathOptions.FromConfiguration(app.Configuration));
var imagesDirectoryPath = Path.Combine(app.Environment.ContentRootPath, imagesDirectory);
Directory.CreateDirectory(imagesDirectoryPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imagesDirectoryPath),
    RequestPath = "/images",
});

app.UseRouting();

app.UseCors(corsAllowAllName);
app.UseCors(corsAllowCredentialsName);

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard(options: new DashboardOptions
{
    DarkModeEnabled = true,
    Authorization = new[] { new HangfireDashboardAuthorizationFilter() }
});

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id:guid?}");

app.Run();