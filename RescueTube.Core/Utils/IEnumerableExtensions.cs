namespace RescueTube.Core.Utils;

public static class IEnumerableExtensions
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : notnull => source.OfType<T>();
}