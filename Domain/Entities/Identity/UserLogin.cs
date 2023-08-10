using Microsoft.AspNetCore.Identity;

namespace Domain.Entities.Identity;

public class UserLogin : IdentityUserLogin<Guid>
{
    public User? User { get; set; }
}