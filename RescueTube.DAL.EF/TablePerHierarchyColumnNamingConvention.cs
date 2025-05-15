using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace RescueTube.DAL.EF;

public class TablePerHierarchyColumnNamingConvention : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            if (entityType.BaseType is null)
            {
                continue;
            }

            var discriminatorValue = entityType.GetDiscriminatorValue()?.ToString();
            if (discriminatorValue is null)
            {
                continue;
            }

            foreach (var property in entityType.GetDeclaredProperties())
            {
                if (property.GetColumnNameConfigurationSource() == ConfigurationSource.Explicit)
                {
                    continue;
                }

                var propertyName = property.Name;
                property.SetColumnName($"{discriminatorValue}_{propertyName}");
            }
        }
    }
}