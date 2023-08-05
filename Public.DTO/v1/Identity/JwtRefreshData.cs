namespace Public.DTO.v1.Identity;

/// <summary>
/// Required data for refreshing a JWT and refresh token.
/// </summary>
public class JwtRefreshData
{
    /// <summary>
    /// The JWT to refresh.
    /// </summary>
    public string Jwt { get; set; } = default!;
    /// <summary>
    /// The refresh token to use for refreshing, and to replace with a new refresh token.
    /// </summary>
    public string RefreshToken { get; set; } = default!;
    /// <summary>
    /// The amount of time (in seconds) that the JWT should be valid for.
    /// Must be longer than 1 second and lower than some maximum value (configured in app settings).
    /// </summary>
    public int? ExpiresInSeconds { get; set; }
}