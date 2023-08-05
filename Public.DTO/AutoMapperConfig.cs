using AutoMapper;
using Microsoft.AspNetCore.Http;

#pragma warning disable CS1591

namespace Public.DTO;

public class AutoMapperConfig : Profile
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AutoMapperConfig(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;

        CreateMap<BLL.DTO.Entities.Identity.User, v1.Identity.User>();
    }

    private string GetWebsiteUrl()
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request == null) throw new ApplicationException($"Can't get website base URL, {nameof(HttpContext)} is null");
        return $"{request.Scheme}://{request.Host}{request.PathBase}";
    }
}