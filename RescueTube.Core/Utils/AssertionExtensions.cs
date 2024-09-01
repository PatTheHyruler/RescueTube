using System.Diagnostics.CodeAnalysis;

namespace RescueTube.Core.Utils;

public static class AssertionExtensions
{
    [return: NotNull]
    public static T AssertNotNull<T>(this T? value, string? message = null)
    {
        return value ?? throw new NullReferenceException(message ?? "Value was unexpectedly null");
    }

    public static T AssertNotNull<T>(this T? value, string? message = null) where T : struct
    {
        return value ?? throw new NullReferenceException(message ?? "Value was unexpectedly null");
    }
}