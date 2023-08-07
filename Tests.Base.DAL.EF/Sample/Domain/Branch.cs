using Base.Domain;

namespace Tests.Base.DAL.EF.Sample.Domain;

public class Branch : AbstractIdDatabaseEntity
{
    public Guid TreeId { get; set; }
    public Tree? Tree { get; set; }

    public int Length { get; set; }
    public int? Radius { get; set; }
}