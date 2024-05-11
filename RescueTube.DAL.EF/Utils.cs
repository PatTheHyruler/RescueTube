namespace RescueTube.DAL.EF;

public static class Utils
{
    public static string EscapeWildcards(string value, char escapeChar = '\\')
    {
        value = value.Replace(escapeChar.ToString(), @"\\");

        return new[] { '%', '_', '[', ']', '^' }
            .Aggregate(value, (current, c) =>
                current.Replace(c.ToString(),
                    escapeChar.ToString() + c));
    }
}