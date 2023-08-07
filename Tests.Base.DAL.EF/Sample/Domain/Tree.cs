using Base.Domain;

namespace Tests.Base.DAL.EF.Sample.Domain;

public class Tree : AbstractIdDatabaseEntity
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? NickName { get; set; }

    public ICollection<Branch>? Branches { get; set; }
}