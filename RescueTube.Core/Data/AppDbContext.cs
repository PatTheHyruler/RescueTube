using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RescueTube.Core.Contracts;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Entities.Identity;
using RescueTube.Domain.Entities.Localization;

namespace RescueTube.Core.Data;

public abstract class AppDbContext : IdentityDbContext<User, Role, Guid, UserClaim, UserRole,
    UserLogin, RoleClaim, UserToken>
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Author> Authors => Set<Author>();
    public DbSet<AuthorHistory> AuthorHistories => Set<AuthorHistory>();
    public DbSet<AuthorStatisticSnapshot> AuthorStatisticSnapshots => Set<AuthorStatisticSnapshot>();
    public DbSet<AuthorImage> AuthorImages => Set<AuthorImage>();

    public DbSet<Image> Images => Set<Image>();

    public DbSet<TextTranslation> TextTranslations => Set<TextTranslation>();
    public DbSet<TextTranslationKey> TextTranslationKeys => Set<TextTranslationKey>();

    public DbSet<Video> Videos => Set<Video>();
    public DbSet<Caption> Captions => Set<Caption>();
    public DbSet<VideoAuthor> VideoAuthors => Set<VideoAuthor>();
    public DbSet<VideoCategory> VideoCategories => Set<VideoCategory>();
    public DbSet<VideoFile> VideoFiles => Set<VideoFile>();
    public DbSet<VideoImage> VideoImages => Set<VideoImage>();
    public DbSet<VideoStatisticSnapshot> VideoStatisticSnapshots => Set<VideoStatisticSnapshot>();
    public DbSet<VideoTag> VideoTags => Set<VideoTag>();

    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<CommentHistory> CommentHistories => Set<CommentHistory>();
    public DbSet<CommentStatisticSnapshot> CommentStatisticSnapshots => Set<CommentStatisticSnapshot>();

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<EntityAccessPermission> EntityAccessPermissions => Set<EntityAccessPermission>();
    public DbSet<StatusChangeEvent> StatusChangeEvents => Set<StatusChangeEvent>();

    public DbSet<Submission> Submissions => Set<Submission>();

    public DbSet<Playlist> Playlists => Set<Playlist>();
    public DbSet<PlaylistItem> PlaylistItems => Set<PlaylistItem>();
    public DbSet<PlaylistItemPositionHistory> PlaylistItemPositionHistories => Set<PlaylistItemPositionHistory>();
    public DbSet<PlaylistImage> PlaylistImages => Set<PlaylistImage>();

    public DbSet<DataFetch> DataFetches => Set<DataFetch>();

    private readonly ILoggerFactory? _loggerFactory;
    private readonly DbLoggingOptions? _dbLoggingOptions;

    protected AppDbContext(DbContextOptions options,
        IOptions<DbLoggingOptions> dbLoggingOptions,
        ILoggerFactory? loggerFactory = null) : base(options)
    {
        _loggerFactory = loggerFactory;
        _dbLoggingOptions = dbLoggingOptions.Value;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        foreach (var foreignKey in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }

        builder.ReconfigureIdentity();

        builder.Entity<EntityAccessPermission>()
            .HasOne(e => e.User)
            .WithMany(e => e.EntityAccessPermissions)
            .HasForeignKey(e => e.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Author>()
            .HasOne(e => e.ArchivalSettings)
            .WithOne(e => e.Author)
            .HasForeignKey<Author>(e => e.ArchivalSettingsId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Author>()
            .HasIndex(e => e.ArchivalSettingsId)
            .IsUnique();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseLoggerFactory(_loggerFactory);
        if (_dbLoggingOptions?.SensitiveDataLogging ?? false)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }

    public EntityEntry<TEntity> AddIfTracked<TEntity>(TEntity entity) where TEntity : class
    {
        var entry = Entry(entity);
        if (entry.State != EntityState.Detached)
        {
            entry.State = EntityState.Added;
        }

        return entry;
    }
}

internal static class DbContextConfigurationExtensions
{
    public static void ReconfigureIdentity(this ModelBuilder builder)
    {
        builder.Entity<UserRole>()
            .HasOne(e => e.User)
            .WithMany(e => e.UserRoles)
            .HasForeignKey(e => e.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserRole>()
            .HasOne(e => e.Role)
            .WithMany(e => e.UserRoles)
            .HasForeignKey(e => e.RoleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserClaim>()
            .HasOne(e => e.User)
            .WithMany(e => e.UserClaims)
            .HasForeignKey(e => e.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RoleClaim>()
            .HasOne(e => e.Role)
            .WithMany(e => e.RoleClaims)
            .HasForeignKey(e => e.RoleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserLogin>()
            .HasOne(e => e.User)
            .WithMany(e => e.UserLogins)
            .HasForeignKey(e => e.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserToken>()
            .HasOne(e => e.User)
            .WithMany(e => e.UserTokens)
            .HasForeignKey(e => e.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}