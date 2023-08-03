using DAL.Contracts;
using DAL.Contracts.Repositories.Identity;
using DAL.EF.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DAL.EF.Repositories.Identity;

public class UserRepository : BaseAppEntityRepository<Domain.Identity.User>, IUserRepository
{
    public async Task<ICollection<Domain.Identity.User>> GetAllTest()
    {
        return await Entities.ToListAsync();
    }

    public UserRepository(AbstractAppDbContext dbContext, IAppUnitOfWork uow) : base(dbContext, uow)
    {
    }
}