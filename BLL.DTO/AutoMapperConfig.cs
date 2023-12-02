using AutoMapper;
using BLL.DTO.Entities;
using Domain.Entities;
using Domain.Enums;

namespace BLL.DTO;

public class AutoMapperConfig : Profile
{
    public AutoMapperConfig()
    {
        AddVideoMap();
        AddAuthorMap();
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
}