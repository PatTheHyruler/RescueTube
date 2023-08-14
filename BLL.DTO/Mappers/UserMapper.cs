using BLL.DTO.Entities.Identity;
using Riok.Mapperly.Abstractions;

namespace BLL.DTO.Mappers;

[Mapper]
public static partial class UserMapper
{
    public static partial Domain.Entities.Identity.User ToDomainUser(
        this User user);
}