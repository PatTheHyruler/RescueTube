using Contracts.Domain;
using Microsoft.AspNetCore.Identity;

namespace Domain.Identity;

public class User : IdentityUser<Guid>, IIdDatabaseEntity
{
    public bool IsApproved { get; set; }

    public ICollection<UserRole>? UserRoles { get; set; }
}