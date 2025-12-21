using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RescueTube.Domain.Entities;

namespace RescueTube.DAL.EF.Configurations;

public class PersistedJobSettingsConfiguration : IEntityTypeConfiguration<PersistedJobSettings>
{
    public void Configure(EntityTypeBuilder<PersistedJobSettings> builder)
    {
        builder.HasIndex(x => x.JobId).IsUnique();

        builder.OwnsOne(x => x.DataFetchJobSettings);
    }
}