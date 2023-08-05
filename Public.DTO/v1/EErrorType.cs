namespace Public.DTO.v1;

/// <summary>
/// Identifiers for the different types of error responses returned by the API.
/// </summary>
public enum EErrorType
{
    /// <summary>
    /// Unspecified generic error.
    /// </summary>
    Unspecified,
    /// <summary>
    /// An error occurred during an identity registration operation (like user account creation).
    /// Typically contains sub-errors with more specific information.
    /// </summary>
    IdentityRegistrationError,
    /// <summary>
    /// User login was attempted with invalid login credentials.
    /// </summary>
    InvalidLoginCredentials,
    /// <summary>
    /// User login was attempted, but the user account must be approved before it can be used.
    /// </summary>
    UserNotApproved,
    /// <summary>
    /// Provided JWT was invalid.
    /// </summary>
    InvalidJwt,
    /// <summary>
    /// Provided refresh token was invalid.
    /// </summary>
    InvalidRefreshToken,
    /// <summary>
    /// Requested JWT expiration time was invalid.
    /// </summary>
    InvalidJwtExpirationRequested,
}