using DAL.DTO.Entities.Identity;

namespace DAL.DTO;
using AutoMapper;

public class AutoMapperConfig : Profile
{
    public AutoMapperConfig()
    {
        CreateMap<Domain.Identity.User, User>().ReverseMap();
        CreateMap<Domain.Identity.RefreshToken, RefreshToken>().ReverseMap();
        // TODO: Figure out if it's necessary to map ID back or if EF manages it
    }
}