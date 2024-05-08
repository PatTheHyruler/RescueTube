using System.Reflection;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApp.Utils;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _descriptionProvider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider descriptionProvider)
    {
        _descriptionProvider = descriptionProvider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _descriptionProvider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                description.GroupName,
                new OpenApiInfo
                {
                    Title = $"API {description.ApiVersion}",
                    Version = description.ApiVersion.ToString(),
                    // Description = , TermsOfService = , Contact = , License =
                }
            );
        }

        // Use FQN for DTO descriptions
        options.CustomSchemaIds(t => t.FullName);

        // Include XML comments
        var xmlFiles = new[]
        {
            $"{Assembly.GetExecutingAssembly().GetName().Name}.xml",
        };
        foreach (var xmlFile in xmlFiles)
        {
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
        }

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT authorization",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header
                },
                new List<string>()
            }
        });
    }
}