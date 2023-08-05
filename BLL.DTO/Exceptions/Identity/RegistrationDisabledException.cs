namespace BLL.DTO.Exceptions.Identity;

public class RegistrationDisabledException : ApplicationException
{
    public RegistrationDisabledException() : base("Account registration has been disabled")
    {
    }
}