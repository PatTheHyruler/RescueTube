using Microsoft.AspNetCore.Identity;

namespace RescueTube.Domain.Entities.Identity;

public class UserLogin : IdentityUserLogin<Guid>
{
    public User? User { get; set; }
}