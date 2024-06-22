using Microsoft.Extensions.DependencyInjection;
using RescueTube.Core.Data;
using RescueTube.Core.Services;

namespace RescueTube.Core;

public class ServiceUow
{
    private readonly IServiceProvider _services;

    public ServiceUow(IServiceProvider services)
    {
        _services = services;
    }

    private IDataUow? _dataUow;
    public IDataUow DataUow => _dataUow ??= _services.GetRequiredService<IDataUow>();

    public SubmissionService SubmissionService => _services.GetRequiredService<SubmissionService>();
    public AuthorizationService AuthorizationService => _services.GetRequiredService<AuthorizationService>();

    public ImageService ImageService => _services.GetRequiredService<ImageService>();
    public StatusChangeService StatusChangeService => _services.GetRequiredService<StatusChangeService>();

    public AuthorService AuthorService => _services.GetRequiredService<AuthorService>();
    public VideoService VideoService => _services.GetRequiredService<VideoService>();
    public CommentService CommentService => _services.GetRequiredService<CommentService>();

    public EntityUpdateService EntityUpdateService => _services.GetRequiredService<EntityUpdateService>();
    
    public VideoPresentationService VideoPresentationService =>
        _services.GetRequiredService<VideoPresentationService>();

    public Task SaveChangesAsync(CancellationToken ct = default) => DataUow.SaveChangesAsync(ct);
}