using Microsoft.EntityFrameworkCore;
using Tests.Base.DAL.EF.Sample.Domain;

namespace Tests.Base.DAL.EF.Sample;

public class SampleDbContext : DbContext
{
    public DbSet<Tree> Trees { get; set; } = default!;
    public DbSet<Branch> Branches { get; set; } = default!;

    public SampleDbContext(DbContextOptions<SampleDbContext> options) : base(options)
    {
    }
}