using Base.Domain;

namespace Tests.Base.DAL.EF.Sample.DTO;

public class TreeLimitedDto : AbstractIdDatabaseEntity
{
    public string Name { get; set; } = default!;
}