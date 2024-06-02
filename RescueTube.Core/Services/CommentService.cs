using RescueTube.Core.Data.Pagination;
using RescueTube.Core.Utils.Pagination;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Base;
using RescueTube.Core.Data.Mappers;
using RescueTube.Core.DTO.Entities;

namespace RescueTube.Core.Services;

public class CommentService : BaseService
{
    public CommentService(IServiceProvider services, ILogger<CommentService> logger) : base(services,
        logger)
    {
    }

    public async Task<PaginationResponse<VideoComments>?> GetVideoComments(Guid videoId, IPaginationQuery paginationQuery,
        CancellationToken ct = default)
    {
        var videoData = await DbCtx.Videos
            .Where(v => v.Id == videoId)
            .Select(v => new
            {
                v.LastCommentsFetch,
            })
            .FirstOrDefaultAsync(cancellationToken: ct);
        if (videoData == null)
        {
            return null;
        }

        paginationQuery = paginationQuery.ToClamped();
        var commentRootsQuery = DbCtx.Comments
            .Where(c => c.VideoId == videoId && c.ConversationRootId == null)
            .OrderByDescending(c => c.CreatedAt)
            .ThenByDescending(c => c.OrderIndex)
            .ThenByDescending(c => c.Id);

        var totalResults = await commentRootsQuery.CountAsync(cancellationToken: ct);

        var commentRoots = await commentRootsQuery
            .Paginate(paginationQuery)
            .Select(EntityMapper.ToCommentDto)
            .ToListAsync(cancellationToken: ct);

        var result = new VideoComments
        {
            Id = videoId,
            LastCommentsFetch = videoData.LastCommentsFetch,
            Comments = commentRoots,
        };

        return new PaginationResponse<VideoComments>
        {
            Result = result,
            PaginationResult = paginationQuery.ToPaginationResult(commentRoots.Count, totalResults),
        };
    }
}