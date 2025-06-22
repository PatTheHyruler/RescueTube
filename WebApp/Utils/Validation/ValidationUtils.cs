using System.Diagnostics.CodeAnalysis;

namespace WebApp.Utils.Validation;

public static class ValidationUtils
{
    [StringSyntax("Regex")]
    public const string LimitedFileNameRegex = @"^[a-zA-Z0-9_\-\.]+$";
}