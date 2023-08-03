using DAL.DTO.Entities.Identity;

namespace DAL.DTO;
using AutoMapper;

public class AutoMapperConfig : Profile
{
    public AutoMapperConfig()
    {
        CreateMap<Domain.Identity.User, User>().ReverseMap();
    }
}