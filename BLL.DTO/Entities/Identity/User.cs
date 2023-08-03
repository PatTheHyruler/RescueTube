using Base.Domain;

namespace BLL.DTO.Entities.Identity;

public class User : AbstractIdDatabaseEntity
{
    public string UserName { get; set; } = default!;
    public bool IsApproved { get; set; }
}