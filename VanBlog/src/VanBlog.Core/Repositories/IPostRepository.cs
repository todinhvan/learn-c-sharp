using VanBlog.Core.Domain.Content;
using VanBlog.Core.Models.Common;
using VanBlog.Core.Models.Content;
using VanBlog.Core.SeedWorks;

namespace VanBlog.Core.Repositories
{
    public interface IPostRepository : IRepository<Post, Guid>
    {
        Task<List<PostInListDto>> GetPopularPostsAsync(int take);
        Task<PaginationResponseDto<PostInListDto>> GetPagedPostsAsync(PaginationRequestDto dto);
    }
}
