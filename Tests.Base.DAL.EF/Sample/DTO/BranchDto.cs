using Base.Domain;

namespace Tests.Base.DAL.EF.Sample.DTO;

public class BranchDto : AbstractIdDatabaseEntity
{
    public int Length { get; set; }
    public int? Radius { get; set; }
}