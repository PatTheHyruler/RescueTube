namespace BLL.Identity.Options;

public class IdentitySeedingOptions
{
    public const string Section = "SeedIdentity";

    public List<UserOptions>? Users { get; set; }

    public class UserOptions
    {
        public string UserName { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string[]? Roles { get; set; }
    }
}