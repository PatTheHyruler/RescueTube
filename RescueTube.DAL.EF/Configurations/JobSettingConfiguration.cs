using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RescueTube.Domain.Entities;

namespace RescueTube.DAL.EF.Configurations;

public class JobSettingConfiguration : IEntityTypeConfiguration<JobSettings>
{
    public void Configure(EntityTypeBuilder<JobSettings> builder)
    {
        builder.HasIndex(x => x.JobId).IsUnique();
    }
}