using Microsoft.EntityFrameworkCore;
using RescueTube.Domain.Base;

namespace RescueTube.Domain.Entities.Localization;

[Index(nameof(KeyId), nameof(Culture), nameof(ValidUntil), nameof(ValidSince))]
public class TextTranslation : BaseIdDbEntity
{
    public required string Content { get; set; }
    public string? Culture { get; set; }

    public DateTime? ValidSince { get; set; }
    public DateTime? ValidUntil { get; set; }

    public Guid KeyId { get; set; }
    public TextTranslationKey? Key { get; set; }
}