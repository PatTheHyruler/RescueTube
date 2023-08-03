using Contracts.DAL;
using Microsoft.EntityFrameworkCore;

namespace Base.DAL.EF;

public class BaseUnitOfWork<TDbContext> : IBaseUnitOfWork where TDbContext : DbContext
{
    protected readonly TDbContext DbContext;

    public BaseUnitOfWork(TDbContext dbContext)
    {
        DbContext = dbContext;
    }
    
    private void BaseDispose()
    {
        GC.SuppressFinalize(this);
    }
    
    public void Dispose()
    {
        DbContext.Dispose();
        BaseDispose();
    }
    
    public async ValueTask DisposeAsync()
    {
        await DbContext.DisposeAsync();
        BaseDispose();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await DbContext.SaveChangesAsync();
    }
}