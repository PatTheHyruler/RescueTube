using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using RescueTube.WebApi.ApiModels;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RescueTube.WebApi.Utils.Swagger;

public class CommaSeparatedGuidArrayParameterFilter : IParameterFilter
{
    public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
    {
        if (context.ParameterInfo?.ParameterType == typeof(CommaSeparatedGuidArray))
        {
            parameter.Schema.Type = "array";
            parameter.Schema.Items = new OpenApiSchema
            {
                Type = "string",
                Format = "uuid",
            };
            parameter.Example = new OpenApiArray();
            parameter.Style = ParameterStyle.Form;
            parameter.Explode = false;
        }
    }
}