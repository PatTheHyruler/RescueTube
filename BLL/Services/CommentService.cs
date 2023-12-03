using BLL.Base;
using BLL.DTO.Entities;
using DAL.EF.Pagination;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utils.Pagination;
using Utils.Pagination.Contracts;

namespace BLL.Services;

public class CommentService : BaseService
{
    public CommentService(IServiceProvider services, ILogger<CommentService> logger) : base(services, logger)
    {
    }
    
    public async Task<VideoComments?> GetVideoComments(Guid videoId, IPaginationQuery paginationQuery, CancellationToken ct = default)
    {
        var videoData = await Ctx.Videos
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

        var result = new VideoComments
        {
            Id = videoId,
            LastCommentsFetch = videoData.LastCommentsFetch,
        };
        
        paginationQuery.ConformValues();
        var commentRootsQuery = Ctx.Comments
            .Where(c => c.VideoId == videoId && c.ConversationRootId == null)
            .OrderByDescending(c => c.CreatedAt)
            .ThenByDescending(c => c.OrderIndex)
            .ThenByDescending(c => c.Id);

        paginationQuery.Total = await commentRootsQuery.CountAsync(cancellationToken: ct);
            
        var commentRoots = await commentRootsQuery
            .Paginate(paginationQuery)
            .ToListAsync(cancellationToken: ct);

        result.Comments = commentRoots;

        return result;
    }
}