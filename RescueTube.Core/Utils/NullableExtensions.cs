using System.Diagnostics.CodeAnalysis;

namespace RescueTube.Core.Utils;

public static class NullableExtensions
{
    [return: NotNull]
    public static T AssertNotNull<T>(this T? value)
    {
        return value ?? throw new ArgumentNullException(nameof(value));
    }

    public static T AssertNotNull<T>(this T? value) where T : struct
    {
        return value ?? throw new ArgumentNullException(nameof(value));
    }
}