namespace RescueTube.Core.Identity.Exceptions;

public class RegistrationDisabledException : ApplicationException
{
    public RegistrationDisabledException() : base("Account registration has been disabled")
    {
    }
}