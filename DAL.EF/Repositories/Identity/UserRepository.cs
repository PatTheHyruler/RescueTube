using AutoMapper;
using DAL.Contracts;
using DAL.Contracts.Repositories.Identity;
using DAL.EF.DbContexts;
using DAL.DTO.Entities.Identity;

namespace DAL.EF.Repositories.Identity;

public class UserRepository : BaseAppEntityRepository<Domain.Identity.User, User>, IUserRepository
{
    public UserRepository(AbstractAppDbContext dbContext, IMapper mapper,
        IAppUnitOfWork uow) : base(dbContext, mapper, uow)
    {
    }
}