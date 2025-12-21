using System.Diagnostics.CodeAnalysis;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using RescueTube.WebApi.ApiModels;

namespace RescueTube.WebApi.Utils.Validation;

public static class ValidationUtils
{
    [StringSyntax("Regex")]
    public const string LimitedFileNameRegex = @"^[a-zA-Z0-9_\-][a-zA-Z0-9_\-\.]*$";

    public static bool IsValid(this ValidationResult validationResult, [NotNullWhen(false)] out BadRequest<ErrorResponseDto>? response)
    {
        if (validationResult.IsValid)
        {
            response = null;
            return true;
        }

        response = TypedResults.BadRequest(new ErrorResponseDto
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
        return false;
    }
}