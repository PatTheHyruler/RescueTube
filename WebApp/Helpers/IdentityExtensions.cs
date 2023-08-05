using BLL.DTO.Exceptions.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Public.DTO.v1.Identity;

namespace WebApp.Helpers;

/// <summary>
/// Extension methods to make identity-related operations more convenient.
/// </summary>
public static class IdentityExtensions
{
    /// <summary>
    /// Get the appropriate <see cref="IActionResult"/> for an <see cref="IdentityOperationFailedException"/>.
    /// </summary>
    /// <param name="exception"></param>
    /// <returns>An <see cref="IActionResult"/> containing a fitting <see cref="IdentityRegistrationErrorResponse"/>.</returns>
    public static IActionResult ToActionResult(this IdentityOperationFailedException exception)
    {
        return exception.Errors.ToActionResult();
    }

    private static IActionResult ToActionResult(this IEnumerable<IdentityError> errors)
    {
        var convertedErrors = errors.Select(e => e.ToDtoIdentityError());
        return new BadRequestObjectResult(new IdentityRegistrationErrorResponse
        {
            IdentityErrors = convertedErrors,
        });
    }

    private static IdentityRegistrationError ToDtoIdentityError(
        this IdentityError error)
    {
        var errorType = error.Code switch
        {
            nameof(IdentityErrorDescriber.DefaultError) => EIdentityRegistrationErrorCode.DefaultError,
            nameof(IdentityErrorDescriber.ConcurrencyFailure) => EIdentityRegistrationErrorCode.ConcurrencyFailure,
            nameof(IdentityErrorDescriber.PasswordMismatch) => EIdentityRegistrationErrorCode.PasswordMismatch,
            nameof(IdentityErrorDescriber.InvalidToken) => EIdentityRegistrationErrorCode.InvalidToken,
            nameof(IdentityErrorDescriber.RecoveryCodeRedemptionFailed) => EIdentityRegistrationErrorCode.RecoveryCodeRedemptionFailed,
            nameof(IdentityErrorDescriber.LoginAlreadyAssociated) => EIdentityRegistrationErrorCode.LoginAlreadyAssociated,
            nameof(IdentityErrorDescriber.InvalidUserName) => EIdentityRegistrationErrorCode.InvalidUserName,
            nameof(IdentityErrorDescriber.InvalidEmail) => EIdentityRegistrationErrorCode.InvalidEmail,
            nameof(IdentityErrorDescriber.DuplicateUserName) => EIdentityRegistrationErrorCode.DuplicateUserName,
            nameof(IdentityErrorDescriber.DuplicateEmail) => EIdentityRegistrationErrorCode.DuplicateEmail,
            nameof(IdentityErrorDescriber.InvalidRoleName) => EIdentityRegistrationErrorCode.InvalidRoleName,
            nameof(IdentityErrorDescriber.DuplicateRoleName) => EIdentityRegistrationErrorCode.DuplicateRoleName,
            nameof(IdentityErrorDescriber.UserAlreadyHasPassword) => EIdentityRegistrationErrorCode.UserAlreadyHasPassword,
            nameof(IdentityErrorDescriber.UserLockoutNotEnabled) => EIdentityRegistrationErrorCode.UserLockoutNotEnabled,
            nameof(IdentityErrorDescriber.UserAlreadyInRole) => EIdentityRegistrationErrorCode.UserAlreadyInRole,
            nameof(IdentityErrorDescriber.UserNotInRole) => EIdentityRegistrationErrorCode.UserNotInRole,
            nameof(IdentityErrorDescriber.PasswordTooShort) => EIdentityRegistrationErrorCode.PasswordTooShort,
            nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars) => EIdentityRegistrationErrorCode.PasswordRequiresUniqueChars,
            nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric) => EIdentityRegistrationErrorCode.PasswordRequiresNonAlphanumeric,
            nameof(IdentityErrorDescriber.PasswordRequiresDigit) => EIdentityRegistrationErrorCode.PasswordRequiresDigit,
            nameof(IdentityErrorDescriber.PasswordRequiresLower) => EIdentityRegistrationErrorCode.PasswordRequiresLower,
            nameof(IdentityErrorDescriber.PasswordRequiresUpper) => EIdentityRegistrationErrorCode.PasswordRequiresUpper,
            _ => EIdentityRegistrationErrorCode.DefaultError,
        };
        return new IdentityRegistrationError
        {
            Code = errorType,
            Description = error.Description,
        };
    }
}