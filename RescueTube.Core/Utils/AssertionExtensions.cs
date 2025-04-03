using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace RescueTube.Core.Utils;

public static class AssertionExtensions
{
    public static T AssertNotNull<T>(
        [NotNull] this T? value,
        string message
    ) where T : class
    {
        return value ?? throw new NullReferenceException(message);
    }

    /// <param name="value"></param>
    /// <param name="_">Hack: unused parameter to avoid overload conflict with <see cref="AssertNotNull{T}(T?,string)"/>.</param>
    /// <param name="paramName">Automatically provided name for parameter in caller context.</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T AssertNotNull<T>(
        [NotNull] this T? value,
        int _ = 0,
        [CallerArgumentExpression(nameof(value))] string? paramName = null
    ) where T : class
    {
        return value.AssertNotNull(message: $"{paramName ?? "Value"} was unexpectedly null");
    }

    public static T AssertNotNull<T>(
        [NotNull] this T? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null
    ) where T : struct
    {
        return value ?? throw new NullReferenceException($"{paramName ?? "Value"} was unexpectedly null");
    }
}