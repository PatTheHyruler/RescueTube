using AutoMapper;
using Base.DAL.EF;
using Tests.Base.DAL.EF.Sample.Repositories;

namespace Tests.Base.DAL.EF.Sample;

public class AppUow : BaseUnitOfWork<SampleDbContext>
{
    private readonly IMapper _mapper;
    
    public AppUow(SampleDbContext dbContext, IMapper mapper) : base(dbContext)
    {
        _mapper = mapper;
    }

    private TreeRepository? _trees;
    public TreeRepository Trees => _trees ??= new TreeRepository(DbContext, _mapper, this);
}