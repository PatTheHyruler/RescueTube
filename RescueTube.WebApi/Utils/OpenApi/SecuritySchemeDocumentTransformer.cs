using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace RescueTube.WebApi.Utils.OpenApi;

public sealed class SecuritySchemeDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        const string jwtSchemeId = JwtBearerDefaults.AuthenticationScheme;

        var jwtSecurityScheme = new OpenApiSecurityScheme
        {
            BearerFormat = "JWT Bearer",
            Name = "JWT authentication",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = jwtSchemeId,
        };

        var securitySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            [jwtSchemeId] = jwtSecurityScheme,
        };

        document.Components ??= new();
        document.Components.SecuritySchemes = securitySchemes;

        document.Security ??= [];
        document.Security.Add(new OpenApiSecurityRequirement
        {
            { new OpenApiSecuritySchemeReference(jwtSchemeId, document), [] },
        });

        return Task.CompletedTask;
    }
}