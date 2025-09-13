using RescueTube.WebApi.ApiModels;
using Swashbuckle.AspNetCore.Annotations;

namespace RescueTube.WebApi.Utils;

public class SwaggerErrorResponseAttribute(int statusCode)
    : SwaggerResponseAttribute(statusCode, null, typeof(ErrorResponseDto));