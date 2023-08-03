using DAL.Contracts;
using Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DAL.EF.DbContexts;

public class AbstractAppDbContext : IdentityDbContext<User, Role, Guid, IdentityUserClaim<Guid>, UserRole,
    IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
{
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