namespace Public.DTO.v1.Identity;

/// <summary>
/// Information about a JWT and its refresh token
/// </summary>
public class JwtResponse
{
    /// <summary>
    /// JSON Web Token containing information about the authenticated user.
    /// </summary>
    public string Jwt { get; set; } = default!;
    /// <summary>
    /// Token for refreshing its corresponding JWT.
    /// </summary>
    public string RefreshToken { get; set; } = default!;
    /// <summary>
    /// Point in time after which the refresh token can't be used to refresh its JWT anymore.
    /// </summary>
    public DateTime RefreshTokenExpiresAt { get; set; }
}