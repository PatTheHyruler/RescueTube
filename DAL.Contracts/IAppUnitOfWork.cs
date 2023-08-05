using Contracts.DAL;
using DAL.Contracts.Repositories.Identity;

namespace DAL.Contracts;

public interface IAppUnitOfWork : IBaseUnitOfWork
{
    public IUserRepository Users { get; }
    public IRefreshTokenRepository RefreshTokens { get; }
}