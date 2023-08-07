using AutoMapper;
using BLL.DTO.Entities.Identity;

namespace BLL.DTO;

public class AutoMapperConfig : Profile
{
    public AutoMapperConfig()
    {
        CreateMap<DAL.DTO.Entities.Identity.User, User>().ReverseMap();
        CreateMap<Domain.Entities.Identity.User, User>();
        CreateMap<DAL.DTO.Entities.Identity.RefreshToken, RefreshToken>();
    }
}