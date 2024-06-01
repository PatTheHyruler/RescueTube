using Microsoft.AspNetCore.Identity;

namespace WebApp.Auth;

public static class AuthHelpers
{
    public static readonly string[] AllowedPasswordErrors =
    [
        nameof(IdentityErrorDescriber.PasswordMismatch),
        nameof(IdentityErrorDescriber.PasswordRequiresDigit),
        nameof(IdentityErrorDescriber.PasswordRequiresLower),
        nameof(IdentityErrorDescriber.PasswordRequiresUpper),
        nameof(IdentityErrorDescriber.PasswordTooShort),
        nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric),
        nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars)
    ];

    public static readonly string[] AllowedRegisterUsernameErrors =
    [
        nameof(IdentityErrorDescriber.InvalidUserName),
        nameof(IdentityErrorDescriber.DuplicateUserName)
    ];

    public static class CorsPolicies
    {
        public const string CorsAllowCredentials = "CorsAllowCredentials";
        public const string CorsAllowAll = "CorsAllowAll";
    }
}