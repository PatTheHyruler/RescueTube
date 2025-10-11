using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RescueTube.Core.Contracts;
using RescueTube.Core.Data;
using RescueTube.DAL.EF.Converters;
using RescueTube.Domain;
using RescueTube.Domain.Base;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Entities.Identity;
using RescueTube.Domain.Enums;

namespace RescueTube.DAL.EF;

public abstract class BaseAppDbContext : AppDbContext
{
    protected BaseAppDbContext(DbContextOptions options, IOptions<DbLoggingOptions> dbLoggingOptions,
        ILoggerFactory? loggerFactory = null) : base(options, dbLoggingOptions, loggerFactory)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        foreach (var foreignKey in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }

        builder.ReconfigureIdentity();

        builder.Entity<DataFetch>()
            .HasIndex(e => new { e.Platform, e.VideoIdOnPlatform });

        builder.Entity<DataFetch>()
            .HasIndex(e => new { e.Platform, e.PlaylistIdOnPlatform });

        builder.Entity<DataFetch>()
            .HasIndex(e => new { e.Platform, e.AuthorIdOnPlatform });

        builder.Entity<Author>()
            .HasOne(e => e.ArchivalSettings)
            .WithOne(e => e.Author)
            .HasForeignKey<Author>(e => e.ArchivalSettingsId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Author>()
            .HasIndex(e => e.ArchivalSettingsId)
            .IsUnique();

        builder.Entity<Setting>()
            .HasDiscriminator<string>("SettingType");

        var configureBaseIdDbEntityMethod = typeof(BaseAppDbContext).GetTypeInfo().DeclaredMethods
            .Single(m => m.Name == nameof(ConfigureBaseIdDbEntity));
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (entityType.ClrType.IsAssignableTo(typeof(BaseIdDbEntity)))
                configureBaseIdDbEntityMethod.MakeGenericMethod(entityType.ClrType).Invoke(null, [builder]);
        }

        builder.ApplyConfigurationsFromAssembly(typeof(BaseAppDbContext).Assembly);
    }

    private static void ConfigureBaseIdDbEntity<TEntity>(ModelBuilder modelBuilder) where TEntity : BaseIdDbEntity
    {
        modelBuilder.Entity<TEntity>(builder =>
        {
            builder.Property(e => e.Id).ValueGeneratedNever();
        });
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
        configurationBuilder.Properties<EVideoType>()
            .HaveConversion<EnumToStringConverter<EVideoType>>();
        configurationBuilder
            .Properties<ELiveStatus>()
            .HaveConversion<EnumToStringConverter<ELiveStatus>>();
        configurationBuilder
            .Properties<DataFetchStatus>()
            .HaveConversion<EnumToStringConverter<DataFetchStatus>>();

        configurationBuilder.Properties<DateTimeOffset>()
            .HaveConversion<DateTimeOffsetToUtcConverter>();

        configurationBuilder.Properties<DataSize>()
            .HaveConversion<DataSizeToLongConverter>();

        configurationBuilder.Conventions.Add(_ => new TablePerHierarchyColumnNamingConvention());
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