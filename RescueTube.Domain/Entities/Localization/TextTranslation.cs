using Microsoft.EntityFrameworkCore;
using RescueTube.Domain.Base;

namespace RescueTube.Domain.Entities.Localization;

[Index(nameof(KeyId), nameof(Culture), nameof(ValidUntil), nameof(ValidSince))]
public class TextTranslation : BaseIdDbEntity
{
    public required string Content { get; set; }
    public string? Culture { get; set; }

    public DateTimeOffset? ValidSince { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }

    public Guid KeyId { get; set; }
    public TextTranslationKey? Key { get; set; }
}