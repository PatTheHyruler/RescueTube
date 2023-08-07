using Base.Domain;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities.Localization;

[Index(nameof(KeyId), nameof(Culture), nameof(ValidUntil), nameof(ValidSince))]
public class TextTranslation : AbstractIdDatabaseEntity
{
    public string Content { get; set; } = default!;
    public string? Culture { get; set; }

    public DateTime? ValidSince { get; set; }
    public DateTime? ValidUntil { get; set; }

    public Guid KeyId { get; set; }
    public TextTranslationKey? Key { get; set; }
}