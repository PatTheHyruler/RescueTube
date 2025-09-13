namespace WebApp.ApiModels;

public class TextTranslationDtoV1
{
    public required Guid Id { get; set; }
    public required string Content { get; set; }
    public string? Culture { get; set; }

    public DateTimeOffset? ValidSince { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }
}