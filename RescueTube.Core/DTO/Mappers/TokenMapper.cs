using RescueTube.Core.DTO.Entities.Identity;

namespace RescueTube.Core.DTO.Mappers;

public static class TokenMapper
{
    public static Domain.Entities.Identity.RefreshToken ToDomainToken(this RefreshToken token)
    {
        return new()
        {
            UserId = token.UserId,
            Token = token.Token,
            ExpiresAt = token.ExpiresAt,
            JwtHash = token.JwtHash,
        };
    }
}