namespace BLL.Identity.Options;

public class IdentitySeedingOptions
{
    public const string Section = "SeedIdentity";

    public List<UserOptions>? Users { get; set; }

    public class UserOptions
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public string[]? Roles { get; set; }
    }
}