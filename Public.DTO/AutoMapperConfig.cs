using AutoMapper;
using Microsoft.AspNetCore.Http;
using Public.DTO.Extensions;

#pragma warning disable CS1591

namespace Public.DTO;

public class AutoMapperConfig : Profile
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AutoMapperConfig(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;

        this.AddUserMap();
    }

    private string GetWebsiteUrl()
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request == null) throw new ApplicationException($"Can't get website base URL, {nameof(HttpContext)} is null");
        return $"{request.Scheme}://{request.Host}{request.PathBase}";
    }
}