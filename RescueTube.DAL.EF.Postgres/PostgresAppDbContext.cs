using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RescueTube.Core.Contracts;
using RescueTube.Domain.Entities;

namespace RescueTube.DAL.EF.Postgres;

public class PostgresAppDbContext : AppDbContext
{
    public PostgresAppDbContext(DbContextOptions<PostgresAppDbContext> options,
        IOptions<DbLoggingOptions> dbLoggingOptions, ILoggerFactory? loggerFactory = null) :
        base(options, dbLoggingOptions, loggerFactory)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Video>().Property(v => v.InfoJson).HasColumnType("jsonb");
    }
}