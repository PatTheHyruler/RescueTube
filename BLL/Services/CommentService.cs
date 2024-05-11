using AutoMapper;
using AutoMapper.QueryableExtensions;
using BLL.Base;
using BLL.Data.Pagination;
using BLL.DTO.Entities;
using BLL.Utils.Pagination;
using BLL.Utils.Pagination.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

public class CommentService : BaseService
{
    private readonly IMapper _mapper;

    public CommentService(IServiceProvider services, ILogger<CommentService> logger, IMapper mapper) : base(services,
        logger)
    {
        _mapper = mapper;
    }

    public async Task<VideoComments?> GetVideoComments(Guid videoId, IPaginationQuery paginationQuery,
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

        paginationQuery.ConformValues();
        var commentRootsQuery = DbCtx.Comments
            .Where(c => c.VideoId == videoId && c.ConversationRootId == null)
            .OrderByDescending(c => c.CreatedAt)
            .ThenByDescending(c => c.OrderIndex)
            .ThenByDescending(c => c.Id);

        paginationQuery.Total = await commentRootsQuery.CountAsync(cancellationToken: ct);

        var commentRoots = await commentRootsQuery
            .Paginate(paginationQuery)
            .ProjectTo<CommentDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken: ct);

        var result = new VideoComments
        {
            Id = videoId,
            LastCommentsFetch = videoData.LastCommentsFetch,
            Comments = commentRoots,
        };

        return result;
    }
}