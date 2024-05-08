using System.ComponentModel.DataAnnotations;

namespace BLL.Identity.Options;

public class JwtBearerOptions
{
    public const string Section = "Auth:JWT";

    [Required] [MinLength(16)] public required string Key { get; set; }
    [Required] public required string Issuer { get; set; }
    [Required] public required string Audience { get; set; }
    [Range(1, int.MaxValue)] public int ExpiresInSeconds { get; set; } = 60 * 5;
    [Range(1, int.MaxValue)] public int ExpiresInSecondsMax { get; set; } = 60 * 30;
    [Range(1, int.MaxValue)] public int RefreshTokenExpiresInDays { get; set; } = 7;
    [Range(1, int.MaxValue)] public int ExtendOldRefreshTokenExpirationByMinutes { get; set; } = 1;
}