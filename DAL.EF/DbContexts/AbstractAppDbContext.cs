using DAL.Contracts;
using DAL.EF.Converters;
using Domain.Entities;
using Domain.Entities.Identity;
using Domain.Entities.Localization;
using Domain.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DAL.EF.DbContexts;

public class AbstractAppDbContext : IdentityDbContext<User, Role, Guid, UserClaim, UserRole,
    UserLogin, RoleClaim, UserToken>
{
    public DbSet<RefreshToken> RefreshTokens { get; set; } = default!;

    public DbSet<Author> Authors { get; set; } = default!;
    public DbSet<AuthorStatisticSnapshot> AuthorStatisticSnapshots { get; set; } = default!;
    public DbSet<AuthorImage> AuthorImages { get; set; } = default!;

    public DbSet<Image> Images { get; set; } = default!;

    public DbSet<TextTranslation> TextTranslations { get; set; } = default!;
    public DbSet<TextTranslationKey> TextTranslationKeys { get; set; } = default!;

    public DbSet<Video> Videos { get; set; } = default!;
    public DbSet<Caption> Captions { get; set; } = default!;
    public DbSet<VideoAuthor> VideoAuthors { get; set; } = default!;
    public DbSet<VideoCategory> VideoCategories { get; set; } = default!;
    public DbSet<VideoFile> VideoFiles { get; set; } = default!;
    public DbSet<VideoImage> VideoImages { get; set; } = default!;
    public DbSet<VideoStatisticSnapshot> VideoStatisticSnapshots { get; set; } = default!;
    public DbSet<VideoTag> VideoTags { get; set; } = default!;

    public DbSet<Comment> Comments { get; set; } = default!;
    public DbSet<CommentHistory> CommentHistories { get; set; } = default!;
    public DbSet<CommentStatisticSnapshot> CommentStatisticSnapshots { get; set; } = default!;
    
    public DbSet<Category> Categories { get; set; } = default!;
    public DbSet<EntityAccessPermission> EntityAccessPermissions { get; set; } = default!;
    public DbSet<StatusChangeEvent> StatusChangeEvents { get; set; } = default!;

    public DbSet<Submission> Submissions { get; set; } = default!;

    private readonly ILoggerFactory? _loggerFactory;
    private readonly DbLoggingOptions? _dbLoggingOptions;

    public AbstractAppDbContext(DbContextOptions options,
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

        configurationBuilder
            .Properties<DateTime>()
            .HaveConversion<DateTimeConverter>();
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