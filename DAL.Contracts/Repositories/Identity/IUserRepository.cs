using Contracts.DAL;
using DAL.DTO.Entities.Identity;

namespace DAL.Contracts.Repositories.Identity;

public interface IUserRepository : IBaseEntityRepository<Domain.Entities.Identity.User, User>
{
}