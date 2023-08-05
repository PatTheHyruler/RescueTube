namespace Public.DTO.v1.Identity;

/// <summary>
/// An error that occurred in the identity system on account registration.
/// </summary>
public class IdentityRegistrationError
{
    /// <summary>
    /// String identifying the error type with an error code.
    /// </summary>
    public EIdentityRegistrationErrorCode Code { get; set; } = EIdentityRegistrationErrorCode.DefaultError;
    /// <summary>
    /// Description explaining the error code.
    /// </summary>
    public string Description { get; set; } = "";
}