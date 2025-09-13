using System.Reflection;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RescueTube.WebApi.Utils.Swagger;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _descriptionProvider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider descriptionProvider)
    {
        _descriptionProvider = descriptionProvider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        options.UseOneOfForPolymorphism();
        options.UseAllOfForInheritance();
        options.UseAllOfToExtendReferenceSchemas();

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

        options.CustomSchemaIds(type =>
        {
            if (type.IsGenericTypeParameter)
            {
                return $"{type.Name}__for_{GetTypeName(type.DeclaringType)}";
            }
            return GetTypeName(type);

            static string? GetTypeName(Type? type)
            {
                return type?.FullName?.Replace('+', '.');
            }
        });

        options.SelectSubTypesUsing(baseType =>
        {
            return baseType.Assembly.GetTypes().Where(x => x.IsSubclassOf(baseType) && !x.IsAbstract);
        });

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

        var jwtSecurityScheme = new OpenApiSecurityScheme
        {
            BearerFormat = "JWT Bearer",
            Description = "Put your JWT Bearer token in the textbox (without the Bearer prefix)",
            Name = "JWT authentication",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            Reference = new OpenApiReference
            {
                Id = JwtBearerDefaults.AuthenticationScheme,
                Type = ReferenceType.SecurityScheme,
            },
        };
        options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, jwtSecurityScheme);

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            { jwtSecurityScheme, [] },
        });

        options.ParameterFilter<CommaSeparatedGuidArrayParameterFilter>();
    }
}