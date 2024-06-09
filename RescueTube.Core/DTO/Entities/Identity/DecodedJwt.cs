using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace RescueTube.Core.DTO.Entities.Identity;

public record DecodedJwt(ClaimsPrincipal Principal, SecurityToken SecurityToken);