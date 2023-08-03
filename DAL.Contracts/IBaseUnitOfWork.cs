namespace DAL.Contracts;

public interface IBaseUnitOfWork : IDisposable, IAsyncDisposable
{
    public Task<int> SaveChangesAsync();
}