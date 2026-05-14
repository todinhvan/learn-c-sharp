using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VanBlog.Core.Domain.Content;
using VanBlog.Core.Models.Common;
using VanBlog.Core.Models.Content;
using VanBlog.Core.Repositories;
using VanBlog.Infrastructure.Data;
using VanBlog.Infrastructure.SeedWorks;

namespace VanBlog.Infrastructure.Repositories
{
    public class PostRepository : Repository<Post, Guid>, IPostRepository
    {
        private readonly IMapper _mapper;

        public PostRepository(VanBlogDbContext context, IMapper mapper) : base(context)
        {
            _mapper = mapper;
        }

        public async Task<PaginationResponseDto<PostInListDto>> GetPagedPostsAsync(PaginationRequestDto dto)
        {
            var queryable = _context.Posts.AsQueryable();
            var skip = (dto.PageIndex - 1) * dto.PageSize;
            var take = dto.PageSize;
            if (!string.IsNullOrEmpty(dto.Keyword))
            {
                queryable.Where(p => p.Name.Contains(dto.Keyword));
            }
            if (dto.CategoryId.HasValue)
            {
                queryable.Where(p => p.CategoryId == dto.CategoryId.Value);
            }
            queryable = queryable.OrderByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Take(take);

            var totalItems = await _context.Posts.CountAsync();
            var items = await _mapper.ProjectTo<PostInListDto>(queryable).ToListAsync();

            return new PaginationResponseDto<PostInListDto>
            {
                PageIndex = dto.PageIndex,
                PageSize = dto.PageSize,
                TotalItems = totalItems,
                Items = items
            };
        }

        public async Task<List<PostInListDto>> GetPopularPostsAsync(int take)
        {
            var queryable = _context.Posts.OrderByDescending(p => p.ViewCount).Take(take);
            return await _mapper.ProjectTo<PostInListDto>(queryable).ToListAsync();
        }
    }
}
