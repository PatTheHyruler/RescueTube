using AutoMapper;
using DAL.Contracts;
using DAL.Contracts.Repositories.Identity;
using DAL.EF.DbContexts;
using Microsoft.EntityFrameworkCore;
using DAL.DTO.Entities.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DAL.EF.Repositories.Identity;

public class UserRepository : BaseAppEntityRepository<Domain.Identity.User, User>, IUserRepository
{
    public async Task<ICollection<Domain.Identity.User>> GetAllTest()
    {
        return await Entities.ToListAsync();
    }

    protected UserRepository(AbstractAppDbContext dbContext, IMapper mapper, IAppUnitOfWork uow) : base(dbContext, mapper, uow)
    {
    }
}

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserRepository(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IUserRepository, UserRepository>();
        return serviceCollection;
    }
}