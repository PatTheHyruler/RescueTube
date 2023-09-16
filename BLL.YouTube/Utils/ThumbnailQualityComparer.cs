namespace BLL.YouTube.Utils;

public class ThumbnailQualityComparer : IComparer<string?>
{
    private static readonly List<string> Qualities = ThumbnailQuality.AllQualityNames.Reverse().ToList();
    
    public int Compare(string? x, string? y)
    {
        if (x == y) return 0;

        var xIndex = x == null ? -1 : Qualities.IndexOf(x);
        var yIndex = y == null ? -1 : Qualities.IndexOf(y);
        if (xIndex == -1 && yIndex == -1) return string.Compare(x, y, StringComparison.Ordinal);
        if (xIndex == -1) return -1;
        if (yIndex == -1) return 1;
        return yIndex - xIndex;
    }
}