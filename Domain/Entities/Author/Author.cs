using Base.Domain;
using Domain.Contracts;
using Domain.Entities.Localization;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Index(nameof(Platform), nameof(IdOnPlatform))]
public class Author : AbstractIdDatabaseEntity, IMainArchiveEntity
{
    public string? UserName { get; set; }
    public string? DisplayName { get; set; }

    public ICollection<AuthorStatisticSnapshot>? AuthorStatisticSnapshots { get; set; }

    public Guid BioId { get; set; }
    public TextTranslationKey? Bio { get; set; }

    public ICollection<AuthorImage>? AuthorImages { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public EPlatform Platform { get; set; }
    public string IdOnPlatform { get; set; } = default!;

    public EPrivacyStatus? PrivacyStatusOnPlatform { get; set; }
    public bool IsAvailable { get; set; }
    public EPrivacyStatus PrivacyStatus { get; set; }

    public DateTime? LastFetchUnofficial { get; set; }
    public DateTime? LastSuccessfulFetchUnofficial { get; set; }
    public DateTime? LastFetchOfficial { get; set; }
    public DateTime? LastSuccessfulFetchOfficial { get; set; }
    public DateTime AddedToArchiveAt { get; set; }
}