using AutoMapper;
using RescueTube.Core.DTO.Entities;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.DTO;

public class AutoMapperConfig : Profile
{
    public AutoMapperConfig()
    {
        AddVideoMap();
        AddAuthorMap();
        AddCommentMap();
    }

    private void AddVideoMap()
    {
        CreateMap<Video, VideoSimple>()
            .ForMember(vs => vs.Title, o =>
                o.MapFrom(v => v.Title!.Translations))
            .ForMember(vs => vs.Description, o =>
                o.MapFrom(v => v.Description!.Translations))
            .ForMember(vs => vs.Thumbnails, o =>
                o.MapFrom(v => v.VideoImages!
                        .Where(vi => vi.ImageType == EImageType.Thumbnail)
                        .Select(vi => vi.Image!)
                ))
            .ForMember(vs => vs.Authors, o =>
                o.MapFrom(v => v.VideoAuthors));
    }
    
    private void AddAuthorMap()
    {
        CreateMap<Author, AuthorSimple>()
            .ForMember(a => a.ProfileImages, o =>
                o.MapFrom(a => a.AuthorImages!
                    .Where(ai => ai.ImageType == EImageType.ProfilePicture)
                    .Select(ai => ai.Image!)
                ));
        CreateMap<VideoAuthor, AuthorSimple>()
            .IncludeMembers(va => va.Author);
    }

    private void AddCommentMap()
    {
        CreateMap<Video, VideoComments>();
        CreateMap<Comment, CommentDto>()
            .ForMember(c => c.Statistics, o => o
                .MapFrom(c => c.CommentStatisticSnapshots!
                    .OrderByDescending(s => s.ValidAt)
                    .FirstOrDefault()));
        CreateMap<CommentStatisticSnapshot, CommentStatisticSnapshotDto>();
    }
}