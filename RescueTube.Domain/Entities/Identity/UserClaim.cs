using Microsoft.AspNetCore.Identity;

namespace RescueTube.Domain.Entities.Identity;

public class UserClaim : IdentityUserClaim<Guid>
{
    public User? User { get; set; }
}