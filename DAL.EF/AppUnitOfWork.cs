using DAL.Contracts;
using DAL.Contracts.Repositories.Identity;
using DAL.EF.DbContexts;
using DAL.EF.Repositories.Identity;

namespace DAL.EF;

public class AppUnitOfWork : BaseUnitOfWork<AbstractAppDbContext>, IAppUnitOfWork
{
    public AppUnitOfWork(AbstractAppDbContext dbContext) : base(dbContext)
    {
    }

    private IUserRepository? _users;
    public IUserRepository Users => _users ??= new UserRepository(DbContext, this);
}