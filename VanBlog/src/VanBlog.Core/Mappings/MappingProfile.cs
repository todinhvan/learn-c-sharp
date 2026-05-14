using AutoMapper;
using VanBlog.Core.Domain.Content;
using VanBlog.Core.Models.Content;

namespace VanBlog.Core.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Post, PostDto>();
            CreateMap<Post, PostInListDto>();
            CreateMap<CreateUpdatePostRequestDto, Post>();
        }
    }
}
