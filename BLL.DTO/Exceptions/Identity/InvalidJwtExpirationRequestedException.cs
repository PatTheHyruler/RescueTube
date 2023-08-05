namespace BLL.DTO.Exceptions.Identity;

public class InvalidJwtExpirationRequestedException : ApplicationException
{
    public readonly int RequestedExpiresInSeconds;
    public readonly int MinExpiresInSeconds;
    public readonly int MaxExpiresInSeconds;

    public InvalidJwtExpirationRequestedException(int requestedExpiresInSeconds, int minExpiresInSeconds,
        int maxExpiresInSeconds) : base(
        $"JWT expiration time must be between {minExpiresInSeconds} and {maxExpiresInSeconds} seconds, but {requestedExpiresInSeconds} was requested")
    {
        RequestedExpiresInSeconds = requestedExpiresInSeconds;
        MinExpiresInSeconds = minExpiresInSeconds;
        MaxExpiresInSeconds = maxExpiresInSeconds;
    }
}