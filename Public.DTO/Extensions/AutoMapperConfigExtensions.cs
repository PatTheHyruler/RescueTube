using Public.DTO.v1.Identity;

namespace Public.DTO.Extensions;

internal static partial class AutoMapperConfigExtensions
{
    public static AutoMapperConfig AddUserMap(this AutoMapperConfig config)
    {
        config.CreateMap<BLL.DTO.Entities.Identity.User, User>();
        return config;
    }
}