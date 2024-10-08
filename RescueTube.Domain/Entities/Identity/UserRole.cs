using Microsoft.AspNetCore.Identity;

namespace RescueTube.Domain.Entities.Identity;

public class UserRole : IdentityUserRole<Guid>
{
    public User? User { get; set; }
    public Role? Role { get; set; }
}