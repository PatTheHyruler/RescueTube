namespace Contracts.DAL;

public interface IBaseUnitOfWork : IDisposable, IAsyncDisposable
{
    public Task<int> SaveChangesAsync();
}