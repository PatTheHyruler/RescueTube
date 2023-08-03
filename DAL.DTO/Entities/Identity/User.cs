using Base.Domain;

namespace DAL.DTO.Entities.Identity;

public class User : AbstractIdDatabaseEntity
{
    public string UserName { get; set; } = default!;

    public bool IsApproved { get; set; }
    public string? ConcurrencyStamp { get; set; }
}