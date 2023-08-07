using Contracts.Domain;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities.Identity;

public class User : IdentityUser<Guid>, IIdDatabaseEntity
{
    public bool IsApproved { get; set; }

    public ICollection<UserRole>? UserRoles { get; set; }
}