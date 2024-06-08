using System.Data.Common;

namespace RescueTube.DAL.EF.MigrationUtils;

public interface IDataMigration
{
    public Task MigrateAsync(DbConnection dbConnection);
}