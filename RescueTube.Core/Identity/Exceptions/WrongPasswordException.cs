namespace RescueTube.Core.Identity.Exceptions;

public class WrongPasswordException : ApplicationException
{
    public WrongPasswordException(string username) : base($"Wrong password provided for user {username}")
    {
    }
}