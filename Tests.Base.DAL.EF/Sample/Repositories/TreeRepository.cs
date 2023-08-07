using AutoMapper;
using AutoMapper.QueryableExtensions;
using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;
using Tests.Base.DAL.EF.Sample.Domain;
using Tests.Base.DAL.EF.Sample.DTO;

namespace Tests.Base.DAL.EF.Sample.Repositories;

public class TreeRepository : BaseEntityRepository<Tree, TreeDto, SampleDbContext, AppUow>
{
    public TreeRepository(SampleDbContext dbContext, IMapper mapper, AppUow uow) : base(dbContext, mapper, uow)
    {
    }

    public override void Update(TreeDto entity)
    {
        Update(entity,
            e => e.Name,
            e => e.Description,
            e => e.NickName);
    }

    public void Update(TreeLimitedDto entity)
    {
        Update(entity,
            e => e.Name);
    }

    public Task<TreeLimitedDto?> GetByIdLimitedAsync(Guid id)
    {
        return Entities.Where(e => e.Id == id)
            .ProjectTo<TreeLimitedDto>(Mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
    }
}