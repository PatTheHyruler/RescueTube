namespace WebApp.ApiModels;

/// <summary>
/// Required data for refreshing a JWT and refresh token
/// </summary>
public class RefreshTokenRequestDtoV1
{
    /// <summary>
    /// The JWT to refresh.
    /// </summary>
    public required string Jwt { get; set; }
    /// <summary>
    /// The refresh token to use for refreshing, and to replace with a new refresh token.
    /// </summary>
    public required string RefreshToken { get; set; }
}