namespace Public.DTO.v1.Identity;

/// <summary>
/// Returned by the API when a login attempt fails because the provided credentials are invalid.
/// </summary>
public class InvalidLoginResponse : RestApiErrorResponse
{
    /// <summary>
    /// Construct a new <see cref="InvalidLoginResponse"/>
    /// </summary>
    public InvalidLoginResponse()
    {
        ErrorType = EErrorType.InvalidLoginCredentials;
        Message = "Invalid login credentials";
    }
}