namespace Public.DTO.v1.Identity;

/// <summary>
/// Data for registering a new user account.
/// </summary>
public class RegistrationData
{
    /// <summary>
    /// The unique username for the user account.
    /// </summary>
    public string Username { get; set; } = default!;

    /// <summary>
    /// The password that will be used to log in to the user account.
    /// </summary>
    public string Password { get; set; } = default!;
}