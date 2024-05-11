using Domain.Contracts;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities.Identity;

public class Role : IdentityRole<Guid>, IIdDatabaseEntity
{
    public ICollection<UserRole>? UserRoles { get; set; }
    public ICollection<RoleClaim>? RoleClaims { get; set; }
}