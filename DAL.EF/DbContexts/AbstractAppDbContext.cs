using DAL.Contracts;
using DAL.EF.Converters;
using Domain.Entities;
using Domain.Entities.Identity;
using Domain.Entities.Localization;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DAL.EF.DbContexts;

public class AbstractAppDbContext : IdentityDbContext<User, Role, Guid, IdentityUserClaim<Guid>, UserRole,
    IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
{
    public DbSet<RefreshToken> RefreshTokens { get; set; } = default!;

    public DbSet<Author> Authors { get; set; } = default!;
    public DbSet<AuthorStatisticSnapshot> AuthorStatisticSnapshots { get; set; } = default!;
    public DbSet<AuthorImage> AuthorImages { get; set; } = default!;

    public DbSet<Image> Images { get; set; } = default!;
    
    public DbSet<TextTranslation> TextTranslations { get; set; } = default!;
    public DbSet<TextTranslationKey> TextTranslationKeys { get; set; } = default!;

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

        builder.Entity<UserRole>()
            .HasOne(e => e.User)
            .WithMany(e => e.UserRoles)
            .HasForeignKey(e => e.UserId);

        builder.Entity<UserRole>()
            .HasOne(e => e.Role)
            .WithMany(e => e.UserRoles)
            .HasForeignKey(e => e.RoleId);
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