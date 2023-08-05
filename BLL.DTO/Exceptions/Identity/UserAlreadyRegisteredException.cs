namespace BLL.DTO.Exceptions.Identity;

public class UserAlreadyRegisteredException : ApplicationException
{
    public UserAlreadyRegisteredException(string username) : base(
        $"User with username {username} is already registered")
    {
    }
}