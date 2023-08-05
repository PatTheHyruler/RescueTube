namespace Public.DTO.v1.Identity;

/// <summary>
/// Returned by the API when an error occurs during an identity registration operation (like user account creation).
/// </summary>
public class IdentityRegistrationErrorResponse : RestApiErrorResponse
{
    /// <summary>
    /// Create a new instance of <see cref="IdentityRegistrationErrorResponse"/>
    /// </summary>
    public IdentityRegistrationErrorResponse()
    {
        ErrorType = EErrorType.IdentityRegistrationError;
    }

    /// <summary>
    /// The errors that caused the identity operation to fail.
    /// </summary>
    public IEnumerable<IdentityRegistrationError> IdentityErrors { get; set; } = new IdentityRegistrationError[] { };
}