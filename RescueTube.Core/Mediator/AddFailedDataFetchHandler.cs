using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RescueTube.Core.Data;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Mediator;

public class AddFailedDataFetchHandler : IRequestHandler<AddFailedDataFetchRequest>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public AddFailedDataFetchHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task Handle(AddFailedDataFetchRequest request, CancellationToken cancellationToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var dbCtx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbCtx.DataFetches.Add(new DataFetch
        {
            OccurredAt = request.OccurredAt,
            Success = false,
            Type = request.Type,
            Source = request.Source,
            ShouldAffectValidity = request.ShouldAffectValidity,
            VideoId = request.VideoId,
            AuthorId = request.AuthorId,
            CommentId = request.CommentId,
            PlaylistId = request.PlaylistId,
        });
        await dbCtx.SaveChangesAsync(cancellationToken);
    }
}