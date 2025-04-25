using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Contracts;
using RescueTube.Core.Data;
using RescueTube.Core.Data.Mappers;
using RescueTube.Core.DTO.Entities;

namespace RescueTube.Core.Services;

public class AuthorPresentationService
{
    private readonly IDataUow _dataUow;
    private readonly IEnumerable<IPlatformPresentationHandler> _presentationHandlers;

    public AuthorPresentationService(IDataUow dataUow, IEnumerable<IPlatformPresentationHandler> presentationHandlers)
    {
        _dataUow = dataUow;
        _presentationHandlers = presentationHandlers;
    }

    public async Task<AuthorSimple?> GetAuthorSimpleAsync(Guid authorId, CancellationToken ct)
    {
        var author = await _dataUow.Ctx.Authors
            .Where(a => a.Id == authorId)
            .Select(EntityMapper.ToAuthorSimple)
            .FirstOrDefaultAsync(ct);
        if (author is null)
        {
            return null;
        }

        var presentationHandler = _presentationHandlers.FirstOrDefault(x => x.CanHandle(author));
        presentationHandler?.Handle(author);

        return author;
    }
}