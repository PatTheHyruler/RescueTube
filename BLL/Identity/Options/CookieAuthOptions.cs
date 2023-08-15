namespace BLL.Identity.Options;

public class CookieAuthOptions
{
    public const string Section = "Auth:Cookie";

    public bool SlidingExpiration { get; set; } = true;
    public TimeSpan ExpireTimeSpan { get; set; } = TimeSpan.FromDays(7);
    public TimeSpan ValidationInterval { get; set; } = TimeSpan.FromMinutes(1);
}