using BLL.Services;
using DAL.EF.DbContexts;
using Microsoft.Extensions.DependencyInjection;

namespace BLL;

public class ServiceUow
{
    private readonly IServiceProvider _services;

    public ServiceUow(IServiceProvider services)
    {
        _services = services;
    }

    private AbstractAppDbContext? _ctx;
    public AbstractAppDbContext Ctx => _ctx ??= _services.GetRequiredService<AbstractAppDbContext>();

    public SubmissionService SubmissionService => _services.GetRequiredService<SubmissionService>();
    public AuthorizationService AuthorizationService => _services.GetRequiredService<AuthorizationService>();

    public ImageService ImageService => _services.GetRequiredService<ImageService>();

    public VideoPresentationService VideoPresentationService =>
        _services.GetRequiredService<VideoPresentationService>();

    public Task SaveChangesAsync() => Ctx.SaveChangesAsync();
}