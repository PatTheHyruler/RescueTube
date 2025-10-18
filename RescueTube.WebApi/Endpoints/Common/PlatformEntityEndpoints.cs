using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Data;
using RescueTube.Domain.Contracts;
using RescueTube.Domain.Enums;
using RescueTube.WebApi.ApiModels;

namespace RescueTube.WebApi.Endpoints.Common;

public static class PlatformEntityEndpoints<T> where T : class, IIdDatabaseEntity, IPlatformEntity
{
    public static async Task<Results<Ok<Guid>, NotFound<ErrorResponseDto>>> GetIdByPlatformIdAsync(
        [FromRoute] EPlatform platform, [FromRoute] string idOnPlatform,
        [FromServices] AppDbContext dbContext,
        CancellationToken ct)
    {
        var entityId = await dbContext.Set<T>()
            .Where(v => v.Platform == platform && v.IdOnPlatform == idOnPlatform)
            .Select(v => v.Id)
            .FirstOrDefaultAsync(ct);
        if (entityId != Guid.Empty)
        {
            return TypedResults.Ok(entityId);
        }

        return TypedResults.NotFound(new ErrorResponseDto
        {
            ErrorType = EErrorType.EntityNotFound,
            Message = $"{typeof(T).Name} not found",
        });
    }
}