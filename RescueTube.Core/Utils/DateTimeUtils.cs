namespace RescueTube.Core.Utils;

public static class DateTimeUtils
{
    public static DateTimeOffset? GetLatest(DateTimeOffset? a, DateTimeOffset? b) =>
        // Perhaps should use IEnumerable.Max() and a params[] argument instead?
        a > b
            ? a
            : b ?? a;
}