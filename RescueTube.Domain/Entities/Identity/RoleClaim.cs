using Microsoft.AspNetCore.Identity;

namespace RescueTube.Domain.Entities.Identity;

public class RoleClaim : IdentityRoleClaim<Guid>
{
    public Role? Role { get; set; }
}