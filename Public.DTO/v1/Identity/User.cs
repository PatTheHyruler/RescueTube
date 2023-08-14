using Riok.Mapperly.Abstractions;

namespace Public.DTO.v1.Identity;

/// <summary>
/// Basic information about a user in the archive.
/// </summary>
public class User
{
    /// <summary>
    /// The unique ID of the user.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// A unique human-friendly string identifying the user.
    /// </summary>
    public string UserName { get; set; } = default!;
    /// <summary>
    /// A boolean indicating whether this user account is approved for usage or not.
    /// If false, this account can't be logged into.
    /// </summary>
    public bool IsApproved { get; set; }
}

#pragma warning disable CS1591
[Mapper]
public static partial class UserMapper
{
    public static partial User ToApiV1User(this BLL.DTO.Entities.Identity.User user);
}
#pragma warning restore CS1591
