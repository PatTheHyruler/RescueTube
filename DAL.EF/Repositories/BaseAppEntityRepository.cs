using DAL.Contracts;
using DAL.EF.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DAL.EF.Repositories;

public abstract class BaseAppEntityRepository<TDomainEntity> where TDomainEntity : class
{
    protected BaseAppEntityRepository(AbstractAppDbContext dbContext, IAppUnitOfWork uow)
    {
        DbContext = dbContext;
        Uow = uow;
    }

    protected AbstractAppDbContext DbContext { get; }
    protected IAppUnitOfWork Uow { get; }
    
    protected DbSet<TDomainEntity> Entities =>
        DbContext
            .GetType()
            .GetProperties()
            .FirstOrDefault(pi => pi.PropertyType == typeof(DbSet<TDomainEntity>))
            ?.GetValue(DbContext) as DbSet<TDomainEntity> ??
        throw new ApplicationException(
            $"Failed to fetch DbSet for Entity type {typeof(TDomainEntity)} from {typeof(DbContext)}");
}