namespace DAL.Contracts.Repositories.Identity;

public interface IUserRepository
{
    public Task<ICollection<Domain.Identity.User>> GetAllTest();
}