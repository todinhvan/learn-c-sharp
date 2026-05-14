using VanBlog.Core.Models.Common;
using VanBlog.Core.Models.Content;

namespace VanBlog.Api.Services
{
    public interface IPostsService
    {
        Task<PaginationResponseDto<PostInListDto>> GetPagedPostsAsync(PaginationRequestDto dto);
        Task<PostDto> GetPostAsync(Guid id);
        Task<PostDto> CreatePostAsync(CreateUpdatePostRequestDto dto);
        Task<PostDto> UpdatePostAsync(Guid id, CreateUpdatePostRequestDto dto);
        Task DeletePostAsync(Guid id);
    }
}
