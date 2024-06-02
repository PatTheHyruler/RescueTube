using Swashbuckle.AspNetCore.Annotations;
using WebApp.ApiModels;

namespace WebApp.Utils;

public class SwaggerErrorResponseAttribute(int statusCode)
    : SwaggerResponseAttribute(statusCode, null, typeof(ErrorResponseDto));