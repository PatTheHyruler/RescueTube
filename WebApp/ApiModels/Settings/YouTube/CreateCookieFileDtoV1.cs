using System.ComponentModel.DataAnnotations;

namespace WebApp.ApiModels.Settings.YouTube;

public sealed record CreateCookieFileDtoV1
{
    [MaxLength(8 * 1000)]
    public required string Content { get; init; }

    [MaxLength(30), RegularExpression(@"^[a-zA-Z0-9_\-\.]+$")]
    public string? FileName { get; init; }
}