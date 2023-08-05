using Microsoft.AspNetCore.Identity;

namespace BLL.DTO.Exceptions.Identity;

public class IdentityOperationFailedException : ApplicationException
{
    public readonly IEnumerable<IdentityError> Errors;

    public IdentityOperationFailedException(IEnumerable<IdentityError> errors) : base(
        "Identity operation failed to complete")
    {
        Errors = errors;
    }
}