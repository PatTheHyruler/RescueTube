using Microsoft.AspNetCore.Identity;

namespace RescueTube.Domain.Entities.Identity;

public class UserToken : IdentityUserToken<Guid>
{
    public User? User { get; set; }
}