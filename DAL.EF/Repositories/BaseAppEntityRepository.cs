using AutoMapper;
using Base.DAL.EF;
using Contracts.Domain;
using DAL.Contracts;
using DAL.EF.DbContexts;

namespace DAL.EF.Repositories;

public class BaseAppEntityRepository<TDomainEntity, TEntity> :
    BaseEntityRepository<TDomainEntity, TEntity, AbstractAppDbContext, IAppUnitOfWork>
    where TDomainEntity : class, IIdDatabaseEntity<Guid>
    where TEntity : class, IIdDatabaseEntity<Guid>
{
    protected BaseAppEntityRepository(AbstractAppDbContext dbContext, IMapper mapper, IAppUnitOfWork uow) :
        base(dbContext, mapper, uow)
    {
    }
}