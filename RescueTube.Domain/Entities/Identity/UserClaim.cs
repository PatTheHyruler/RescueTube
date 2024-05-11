using Microsoft.AspNetCore.Identity;

namespace Domain.Entities.Identity;

public class UserClaim : IdentityUserClaim<Guid>
{
    public User? User { get; set; }
}