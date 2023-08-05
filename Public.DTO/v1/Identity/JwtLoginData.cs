namespace Public.DTO.v1.Identity;

/// <summary>
/// Data required for logging in with a user account and receiving a JWT.
/// </summary>
public class JwtLoginData
{
    /// <summary>
    /// The unique username of the user account.
    /// </summary>
    public string Username { get; set; } = default!;
    /// <summary>
    /// The password of the user account.
    /// </summary>
    public string Password { get; set; } = default!;
    /// <summary>
    /// The amount of time (in seconds) that the JWT should be valid for.
    /// Must be longer than 1 second and lower than some maximum value (configured in app settings).
    /// </summary>
    public int? ExpiresInSeconds { get; set; }
}