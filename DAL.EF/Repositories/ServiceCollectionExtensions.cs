using DAL.Contracts.Repositories.Identity;
using DAL.EF.Repositories.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DAL.EF.Repositories;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEfRepositories(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
    }
}