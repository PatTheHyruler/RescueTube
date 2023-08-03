using Asp.Versioning.ApiExplorer;
using DAL.Contracts;
using DAL.EF;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Settings.Configuration;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebApp;
using ApiVersion = Asp.Versioning.ApiVersion;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration, new ConfigurationReaderOptions { SectionName = "Logging:Serilog"})
    .CreateLogger();

// Add services to the container.

builder.Services.AddAppDbContext(builder.Configuration);
builder.Services.AddScoped<IAppUnitOfWork, AppUnitOfWork>();

builder.Services.AddControllers();
builder.Services.AddMvc(); // For Swagger

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

builder.Services.AddSerilog();
var useHttpLogging = builder.Configuration.GetValue<bool>("Logging:HTTP:Enabled");
if (useHttpLogging)
{
    builder.Services.AddHttpLogging(logging => { logging.LoggingFields = HttpLoggingFields.All; });
}

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

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