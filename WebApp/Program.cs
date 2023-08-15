using System.Text.Json.Serialization;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using BLL;
using BLL.Identity;
using DAL.EF;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Settings.Configuration;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebApp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Logging.ClearProviders();
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration, new ConfigurationReaderOptions { SectionName = "Logging:Serilog" })
    .CreateLogger();
builder.Logging.AddSerilog();
builder.Services.AddSingleton(Log.Logger);

var useHttpLogging = builder.Configuration.GetValue<bool>("Logging:HTTP:Enabled");
if (useHttpLogging)
{
    builder.Services.AddHttpLogging(logging => { logging.LoggingFields = HttpLoggingFields.All; });
}

builder.Services.AddDbPersistenceEf(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddMvc();

builder.AddCustomIdentity();
builder.Services.AddBll();

builder.Services.AddLocalization();

var apiVersioningBuilder = builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
});
apiVersioningBuilder.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.SeedIdentity();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id:guid?}");

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint(
            $"/swagger/{description.GroupName}/swagger.json",
            description.GroupName);
    }
});

app.Run();