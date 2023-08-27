namespace BLL.YouTube.Utils;

public record ThumbnailTag(string Identifier)
{
    public static readonly ThumbnailTag Default = new("default");
    public static readonly ThumbnailTag Alt1 = new("1");
    public static readonly ThumbnailTag Alt2 = new("2");
    public static readonly ThumbnailTag Alt3 = new("3");

    public static readonly IReadOnlyList<ThumbnailTag> AllTags = new[]
    {
        Default,
        Alt1,
        Alt2,
        Alt3,
    };

    public static implicit operator string(ThumbnailTag tag) => tag.Identifier;

    public static ThumbnailTag? FromString(string? str) =>
        AllTags.FirstOrDefault(e => string.Equals(e.Identifier, str?.Trim(), StringComparison.OrdinalIgnoreCase));
}