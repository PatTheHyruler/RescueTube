using Microsoft.AspNetCore.Identity;
using RescueTube.Domain.Contracts;

namespace RescueTube.Domain.Entities.Identity;

public class Role : IdentityRole<Guid>, IIdDatabaseEntity
{
    public ICollection<UserRole>? UserRoles { get; set; }
    public ICollection<RoleClaim>? RoleClaims { get; set; }
}