using System.Text.Json.Serialization;
using BLL;
using BLL.Identity;
using BLL.YouTube;
using DAL.EF;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Serilog.Settings.Configuration;

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
    hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore; // Is this a good idea???
});

var useHttpLogging = builder.Configuration.GetValue<bool>("Logging:HTTP:Enabled");
if (useHttpLogging)
{
    builder.Services.AddHttpLogging(logging => { logging.LoggingFields = HttpLoggingFields.All; });
}

builder.Services.AddDbPersistenceEf(builder.Configuration);

builder.Services.AddControllersWithViews(options =>
    {
        options.Filters.Add(typeof(AutoValidateAntiforgeryTokenAttribute));
    })
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddMvc();

builder.AddCustomIdentity();
builder.Services.AddBll();
builder.Services.AddYouTube();

builder.Services.AddLocalization();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.SeedIdentity();
app.SetupYouTube();

app.UseHttpsRedirection();
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id:guid?}");

app.Run();