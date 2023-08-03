using DAL.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DAL.EF.DbContexts;

public class PostgresAppDbContext : AbstractAppDbContext
{
    public PostgresAppDbContext(DbContextOptions<PostgresAppDbContext> options,
        IOptions<DbLoggingOptions> dbLoggingOptions, ILoggerFactory? loggerFactory = null) :
        base(options, dbLoggingOptions, loggerFactory)
    {
    }
}