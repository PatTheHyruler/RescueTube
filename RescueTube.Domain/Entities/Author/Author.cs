using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using RescueTube.Domain.Base;
using RescueTube.Domain.Contracts;
using RescueTube.Domain.Entities.Localization;
using RescueTube.Domain.Enums;

namespace RescueTube.Domain.Entities;

[Index(nameof(Platform), nameof(IdOnPlatform), IsUnique = true)]
public class Author : BaseIdDbEntity, IMainArchiveEntity
{
    public string? UserName { get; set; }
    public string? DisplayName { get; set; }

    public ICollection<AuthorStatisticSnapshot>? AuthorStatisticSnapshots { get; set; }

    public Guid? BioId { get; set; }
    public TextTranslationKey? Bio { get; set; }

    public ICollection<AuthorImage>? AuthorImages { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public EPlatform Platform { get; set; }
    public required string IdOnPlatform { get; set; }

    public EPrivacyStatus? PrivacyStatusOnPlatform { get; set; }
    public EPrivacyStatus PrivacyStatus { get; set; }

    public int FailedExtraDataFetchAttempts { get; set; }
    public DateTimeOffset? LastFetchUnofficial { get; set; }
    public DateTimeOffset? LastSuccessfulFetchUnofficial { get; set; }
    public DateTimeOffset? LastFetchOfficial { get; set; }
    public DateTimeOffset? LastSuccessfulFetchOfficial { get; set; }
    public DateTimeOffset AddedToArchiveAt { get; set; }

    public ICollection<VideoAuthor>? VideoAuthors { get; set; }

    public Guid? ArchivalSettingsId { get; set; }
    public AuthorArchivalSettings? ArchivalSettings { get; set; }

    [InverseProperty(nameof(Category.Creator))]
    public ICollection<Category>? CreatedCategories { get; set; }
    [InverseProperty(nameof(VideoCategory.AssignedBy))]
    public ICollection<VideoCategory>? AssignedVideoCategories { get; set; }

    public ICollection<StatusChangeEvent>? StatusChangeEvents { get; set; }
    public ICollection<EntityAccessPermission>? EntityAccessPermissions { get; set; }
}