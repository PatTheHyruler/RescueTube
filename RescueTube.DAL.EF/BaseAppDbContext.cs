using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RescueTube.Core.Contracts;
using RescueTube.Core.Data;
using RescueTube.DAL.EF.Converters;
using RescueTube.Domain.Enums;

namespace RescueTube.DAL.EF;

public abstract class BaseAppDbContext : AppDbContext
{
    protected BaseAppDbContext(DbContextOptions options, IOptions<DbLoggingOptions> dbLoggingOptions,
        ILoggerFactory? loggerFactory = null) : base(options, dbLoggingOptions, loggerFactory)
    {
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
}