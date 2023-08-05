namespace Public.DTO.v1.Identity;

/// <summary>
/// Possible types for identity registration errors.
/// </summary>
public enum EIdentityRegistrationErrorCode
{
    /// <summary>
    /// Unknown/unspecified error.
    /// </summary>
    DefaultError,
    /// <summary>
    /// A concurrency conflict has occurred.
    /// </summary>
    ConcurrencyFailure,
    /// <summary>
    /// Passwords do not match.
    /// </summary>
    PasswordMismatch,
    /// <summary>
    /// Invalid token.
    /// </summary>
    InvalidToken,
    /// <summary>
    /// Recovery code was not redeemed.
    /// </summary>
    RecoveryCodeRedemptionFailed,
    /// <summary>
    /// Specified external login is already associated with specified account.
    /// </summary>
    LoginAlreadyAssociated,
    /// <summary>
    /// Specified username is invalid.
    /// </summary>
    InvalidUserName,
    /// <summary>
    /// Specified email is invalid.
    /// </summary>
    InvalidEmail,
    /// <summary>
    /// Specified username already exists.
    /// </summary>
    DuplicateUserName,
    /// <summary>
    /// Specified email is already associated with an account.
    /// </summary>
    DuplicateEmail,
    /// <summary>
    /// Specified role name is invalid.
    /// </summary>
    InvalidRoleName,
    /// <summary>
    /// Specified role name already exists.
    /// </summary>
    DuplicateRoleName,
    /// <summary>
    /// Specified user already has a password.
    /// </summary>
    UserAlreadyHasPassword,
    /// <summary>
    /// User lockout is not enabled.
    /// </summary>
    UserLockoutNotEnabled,
    /// <summary>
    /// User is already in specified role.
    /// </summary>
    UserAlreadyInRole,
    /// <summary>
    /// User is not in specified role.
    /// </summary>
    UserNotInRole,
    /// <summary>
    /// Password does not meet the minimum length requirements.
    /// </summary>
    PasswordTooShort,
    /// <summary>
    /// Password does not meet the minimum number of unique characters requirement.
    /// </summary>
    PasswordRequiresUniqueChars,
    /// <summary>
    /// Password does not contain a non-alphanumeric character, which is required by the password policy.
    /// </summary>
    PasswordRequiresNonAlphanumeric,
    /// <summary>
    /// Password does not contain a numeric character, which is required by the password policy.
    /// </summary>
    PasswordRequiresDigit,
    /// <summary>
    /// Password does not contain a lower case letter, which is required by the password policy.
    /// </summary>
    PasswordRequiresLower,
    /// <summary>
    /// Password does not contain an upper case letter, which is required by the password policy.
    /// </summary>
    PasswordRequiresUpper,
    /// <summary>
    /// User account registration is currently disabled.
    /// </summary>
    RegistrationDisabled,
}