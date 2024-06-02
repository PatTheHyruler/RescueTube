using Microsoft.AspNetCore.Identity;

namespace RescueTube.Core.Identity.Exceptions;

public class IdentityOperationFailedException : ApplicationException
{
    public readonly IEnumerable<IdentityError> Errors;

    public IdentityOperationFailedException(IEnumerable<IdentityError> errors) : base(
        "Identity operation failed to complete")
    {
        Errors = errors;
    }
}