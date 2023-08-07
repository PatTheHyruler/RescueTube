using Microsoft.EntityFrameworkCore;
using Tests.Base.DAL.EF.Sample;
using Tests.Base.DAL.EF.Sample.Domain;
using Tests.Base.DAL.EF.Sample.DTO;

namespace Tests.Base.DAL.EF;

public class DtoUpdateTests : BaseTests
{
    private static readonly Tree Oak = new()
    {
        Id = Guid.NewGuid(),
        Name = "Oak",
        Description = "Nice big tree",
        NickName = "Ol' Oaky",
    };

    private const int OakExpectedBranches = 2;

    private async Task ArrangeOak()
    {
        await using var dbContext = NewDbContext();

        var oak = new Tree
        {
            Id = Oak.Id,
            Name = Oak.Name,
            Description = Oak.Description,
            NickName = Oak.NickName,
        };
        dbContext.Add(oak);
        dbContext.Add(new Branch
        {
            Tree = oak,
            Length = 6,
            Radius = 2,
        });
        dbContext.Add(new Branch
        {
            Tree = oak,
            Length = 7,
        });

        await dbContext.SaveChangesAsync();
    }

    private async Task BaseTestPartialUpdate(
        bool repoFetch,
        Action<AppUow> updateFunc,
        EntityState expectedPostUpdateState,
        Tree expectedPostUpdateFetchValues)
    {
        await ArrangeOak();

        await using (var dbContext = NewDbContext())
        {
            var uow = new AppUow(dbContext, Mapper);

            if (repoFetch)
            {
                var oak = await uow.Trees.GetByIdAsync(Oak.Id);
                Assert.NotNull(oak);
            }
            else
            {
                var oak = await dbContext.Trees.FindAsync(Oak.Id);
                Assert.NotNull(oak);
            }

            updateFunc(uow);

            var entry = dbContext.ChangeTracker.Entries<Tree>().Single(e => e.Entity.Id == Oak.Id);
            Assert.Equal(expectedPostUpdateState, entry.State);

            await uow.SaveChangesAsync();
        }

        await using (var dbContext = NewDbContext())
        {
            var oak = await dbContext.Trees
                .Include(e => e.Branches)
                .FirstOrDefaultAsync(e => e.Id == Oak.Id);
            Assert.NotNull(oak);

            Assert.Equal(expectedPostUpdateFetchValues.Name, oak.Name);
            Assert.Equal(expectedPostUpdateFetchValues.Description, oak.Description);
            Assert.Equal(expectedPostUpdateFetchValues.NickName, oak.NickName);
            Assert.NotNull(oak.Branches);
            Assert.Equal(OakExpectedBranches, oak.Branches.Count);
        }
    }

    [Theory]
    [InlineData(true, EntityState.Unchanged)]
    [InlineData(false, EntityState.Modified)]
    public async Task TestUpdateNotModified(bool tracked, EntityState expectedPostUpdateState)
    {
        await BaseTestPartialUpdate(!tracked,
            uow => uow.Trees.Update(Mapper.Map<TreeDto>(Oak)),
            expectedPostUpdateState,
            Oak);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task TestPartialUpdateMissingNullable(bool tracked)
    {
        var tree = new TreeLimitedDto
        {
            Id = Oak.Id,
            Name = "ChangedOakName",
        };
        await BaseTestPartialUpdate(!tracked,
            uow => uow.Trees.Update(tree),
            EntityState.Modified,
            new Tree
            {
                Name = tree.Name,
                Description = Oak.Description,
                NickName = Oak.NickName,
            });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task TestPartialUpdateSetNull(bool tracked)
    {
        var tree = new TreeDto
        {
            Id = Oak.Id,
            Name = "ChangedOakName",
            Description = "ChangedOakDescription",
            NickName = null,
        };
        await BaseTestPartialUpdate(!tracked,
            uow => uow.Trees.Update(tree),
            EntityState.Modified,
            new Tree
            {
                Name = tree.Name,
                Description = tree.Description,
                NickName = null,
            });
    }
}