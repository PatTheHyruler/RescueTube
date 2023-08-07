using Base.Domain;

namespace Domain.Entities.Localization;

public class TextTranslationKey : AbstractIdDatabaseEntity
{
    public ICollection<TextTranslation>? Translations { get; set; }
}