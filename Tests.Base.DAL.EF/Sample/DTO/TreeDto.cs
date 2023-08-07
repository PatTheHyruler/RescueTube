using Base.Domain;

namespace Tests.Base.DAL.EF.Sample.DTO;

public class TreeDto : AbstractIdDatabaseEntity
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? NickName { get; set; }

    public ICollection<BranchDto> Branches { get; set; } = default!;
}