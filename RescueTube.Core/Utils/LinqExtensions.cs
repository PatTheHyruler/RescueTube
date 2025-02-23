namespace RescueTube.Core.Utils;

public static class LinqExtensions
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : struct
    {
        return source.Where(x => x is not null).Cast<T>();
    }
}