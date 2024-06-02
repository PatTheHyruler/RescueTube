using RescueTube.Core.DTO.Entities.Identity;
using Riok.Mapperly.Abstractions;

namespace RescueTube.Core.DTO.Mappers;

[Mapper]
public static partial class TokenMapper
{
    public static partial Domain.Entities.Identity.RefreshToken ToDomainToken(this RefreshToken token);
    public static partial RefreshToken ToBllToken(this Domain.Entities.Identity.RefreshToken token);
}