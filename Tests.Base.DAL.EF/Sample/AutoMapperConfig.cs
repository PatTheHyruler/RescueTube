using AutoMapper;
using Tests.Base.DAL.EF.Sample.Domain;
using Tests.Base.DAL.EF.Sample.DTO;

namespace Tests.Base.DAL.EF.Sample;

public class AutoMapperConfig : Profile
{
    public AutoMapperConfig()
    {
        CreateMap<Branch, BranchDto>().ReverseMap();
        CreateMap<Tree, TreeDto>().ReverseMap();
        CreateMap<Tree, TreeLimitedDto>().ReverseMap();
    }
}