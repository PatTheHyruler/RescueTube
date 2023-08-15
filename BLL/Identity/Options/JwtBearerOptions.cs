using System.ComponentModel.DataAnnotations;
using ConfigDefaults;

namespace BLL.Identity.Options;

public class JwtBearerOptions
{
    public const string Section = "Auth:JWT";

    [Required] [MinLength(16)] public string Key { get; set; } = default!;
    [Required] public string Issuer { get; set; } = default!;
    [Required] public string Audience { get; set; } = default!;
    [Range(1, int.MaxValue)] public int ExpiresInSeconds { get; set; } = 60;
    [Range(1, int.MaxValue)] public int ExpiresInSecondsMax { get; set; } = IdentityDefaults.JwtExpiresInSecondsMax;
    [Range(1, int.MaxValue)] public int RefreshTokenExpiresInDays { get; set; } = 7;
    [Range(1, int.MaxValue)] public int ExtendOldRefreshTokenExpirationByMinutes { get; set; } = 1;
}