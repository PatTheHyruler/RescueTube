using Microsoft.EntityFrameworkCore;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Entities.Identity;
using RescueTube.Domain.Entities.Localization;

namespace RescueTube.Core.Data;

public interface IAppDbContext
{
    public DbSet<RefreshToken> RefreshTokens { get; }
    public DbSet<Author> Authors { get; }
    public DbSet<AuthorStatisticSnapshot> AuthorStatisticSnapshots { get; }
    public DbSet<AuthorImage> AuthorImages { get; }
    public DbSet<Image> Images { get; }
    public DbSet<TextTranslation> TextTranslations { get; }
    public DbSet<TextTranslationKey> TextTranslationKeys { get; }
    public DbSet<Video> Videos { get; }
    public DbSet<Caption> Captions { get; }
    public DbSet<VideoAuthor> VideoAuthors { get; }
    public DbSet<VideoCategory> VideoCategories { get; }
    public DbSet<VideoFile> VideoFiles { get; }
    public DbSet<VideoImage> VideoImages { get; }
    public DbSet<VideoStatisticSnapshot> VideoStatisticSnapshots { get; }
    public DbSet<VideoTag> VideoTags { get; }
    public DbSet<Comment> Comments { get; }
    public DbSet<CommentHistory> CommentHistories { get; }
    public DbSet<CommentStatisticSnapshot> CommentStatisticSnapshots { get; }
    public DbSet<Category> Categories { get; }
    public DbSet<EntityAccessPermission> EntityAccessPermissions { get; }
    public DbSet<StatusChangeEvent> StatusChangeEvents { get; }
    public DbSet<Submission> Submissions { get; }

    public DbSet<User> Users { get; }
    public DbSet<Role> Roles { get; }
    public DbSet<UserRole> UserRoles { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct = default);
}