using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Tests.Base.DAL.EF.Sample;

namespace Tests.Base.DAL.EF;

public class BaseTests : IDisposable
{
    protected Mapper Mapper;
    protected Guid InstanceId;

    public BaseTests()
    {
        Mapper = new Mapper(new MapperConfiguration(exp =>
        {
            exp.AddProfile<AutoMapperConfig>();
        }));

        InstanceId = Guid.NewGuid();

        using var dbContext = NewDbContext();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }

    protected SampleDbContext NewDbContext()
    {
        var options = new DbContextOptionsBuilder<SampleDbContext>();
        options.UseSqlite($"Data Source={Path.Combine(Path.GetTempPath(), $"ef_tests_{InstanceId}.sqlite")};");
        return new SampleDbContext(options.Options);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            using var dbContext = NewDbContext();
            dbContext.Database.EnsureDeleted();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}