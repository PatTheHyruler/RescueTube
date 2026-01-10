using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using RescueTube.WebApi.ApiModels;

namespace RescueTube.WebApi.Utils.OpenApi;

public sealed class CommaSeparatedGuidArraySchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.ParameterDescription?.Type == typeof(CommaSeparatedGuidArray))
        {
            schema.Type = JsonSchemaType.Array;
            schema.Items = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Format = "uuid",
            };
            schema.Example = new JsonArray();
        }

        return Task.CompletedTask;
    }
}