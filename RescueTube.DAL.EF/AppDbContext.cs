using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RescueTube.Core.Contracts;
using RescueTube.Core.Data;
using RescueTube.DAL.EF.Converters;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Entities.Identity;
using RescueTube.Domain.Entities.Localization;
using RescueTube.Domain.Enums;

namespace RescueTube.DAL.EF;

public abstract class AppDbContext : IdentityDbContext<User, Role, Guid, UserClaim, UserRole,
    UserLogin, RoleClaim, UserToken>, IAppDbContext
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Author> Authors => Set<Author>();
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
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder
            .Properties<EPlatform>()
            .HaveConversion<EnumToStringConverter<EPlatform>>();
        configurationBuilder
            .Properties<EImageType>()
            .HaveConversion<EnumToStringConverter<EImageType>>();
        configurationBuilder
            .Properties<EPrivacyStatus>()
            .HaveConversion<EnumToStringConverter<EPrivacyStatus>>();
        configurationBuilder
            .Properties<EAuthorRole>()
            .HaveConversion<EnumToStringConverter<EAuthorRole>>();
        configurationBuilder
            .Properties<EEntityType>()
            .HaveConversion<EnumToStringConverter<EEntityType>>();

        configurationBuilder.Properties<DateTimeOffset>()
            .HaveConversion<DateTimeOffsetToUtcConverter>();
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

    public Task MigrateAsync()
    {
        return Database.MigrateAsync();
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