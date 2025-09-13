using FluentValidation;
using RescueTube.WebApi.ApiModels;

namespace RescueTube.WebApi.Utils.Validation;

public class ValidationFilter<TRequest, TValidator> : IEndpointFilter where TValidator : IValidator<TRequest>
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetRequiredService<TValidator>();
        var data = context.Arguments.OfType<TRequest>().FirstOrDefault();
        if (data is not null)
        {
            var validationResult = await validator.ValidateAsync(data, context.HttpContext.RequestAborted);
            if (!validationResult.IsValid)
            {
                return TypedResults.BadRequest(new ErrorResponseDto
                {
                    ErrorType = EErrorType.ValidationError,
                    Message = "Invalid request data",
                    Details = validationResult.Errors.ToDictionary(v => v.PropertyName, v => new
                    {
                        v.PropertyName,
                        v.AttemptedValue,
                        v.ErrorCode,
                        v.ErrorMessage,
                    }),
                });
            }
        }

        return await next(context);
    }
}

public class ValidationFilter<TRequest> : ValidationFilter<TRequest, IValidator<TRequest>>;