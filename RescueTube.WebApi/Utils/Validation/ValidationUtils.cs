using System.Diagnostics.CodeAnalysis;

namespace RescueTube.WebApi.Utils.Validation;

public static class ValidationUtils
{
    [StringSyntax("Regex")]
    public const string LimitedFileNameRegex = @"^[a-zA-Z0-9_\-][a-zA-Z0-9_\-\.]*$";
}