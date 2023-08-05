namespace ConfigDefaults;

public static class IdentityDefaults
{
    public const int PasswordMinLength = 16;

    public const int JwtExpiresInSecondsMax = 60 * 60 * 24 * 7;
}