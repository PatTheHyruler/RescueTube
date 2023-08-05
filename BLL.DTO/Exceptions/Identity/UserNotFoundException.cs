namespace BLL.DTO.Exceptions.Identity;

public class UserNotFoundException : ApplicationException
{
    public UserNotFoundException(string username) : base($"User with username {username} not found")
    {
    }
}