namespace RescueTube.Core.Identity.Exceptions;

public class UserNotFoundException : ApplicationException
{
    public UserNotFoundException(string username) : base($"User with username {username} not found")
    {
    }
}