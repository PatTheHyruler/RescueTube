namespace RescueTube.DAL.EF.Repositories;

public abstract class BaseRepository
{
    protected readonly AppDbContext Ctx;

    protected BaseRepository(AppDbContext ctx)
    {
        Ctx = ctx;
    }
}