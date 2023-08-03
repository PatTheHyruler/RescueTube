using AutoMapper;
using Base.DAL.EF;
using DAL.Contracts;
using DAL.Contracts.Repositories.Identity;
using DAL.EF.DbContexts;
using DAL.EF.Repositories.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DAL.EF;

public class AppUnitOfWork : BaseUnitOfWork<AbstractAppDbContext>, IAppUnitOfWork
{
    private readonly IServiceProvider _services;
    
    public AppUnitOfWork(AbstractAppDbContext dbContext, IServiceProvider services) : base(dbContext)
    {
        _services = services;
    }

    public IUserRepository Users => _services.GetRequiredService<IUserRepository>();
}