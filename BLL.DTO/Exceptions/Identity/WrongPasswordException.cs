namespace BLL.DTO.Exceptions.Identity;

public class WrongPasswordException : ApplicationException
{
    public WrongPasswordException(string username) : base($"Wrong password provided for user {username}")
    {
    }
}