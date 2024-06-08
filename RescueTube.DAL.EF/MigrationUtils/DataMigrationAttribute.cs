namespace RescueTube.DAL.EF.MigrationUtils;

[AttributeUsage(AttributeTargets.Class)]
public class DataMigrationAttribute : Attribute
{
    public Type DataMigrationType { get; }
    
    public DataMigrationAttribute(Type dataMigrationType)
    {
        if (!typeof(IDataMigration).IsAssignableFrom(dataMigrationType))
        {
            throw new ArgumentException(
                $"Migration type {dataMigrationType.FullName}" +
                $" doesn't implement {typeof(IDataMigration).FullName}",
                nameof(dataMigrationType));
        }
        DataMigrationType = dataMigrationType;
    }
}