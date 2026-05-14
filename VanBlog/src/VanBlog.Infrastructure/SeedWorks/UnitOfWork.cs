using AutoMapper;
using VanBlog.Core.Repositories;
using VanBlog.Core.SeedWorks;
using VanBlog.Infrastructure.Data;
using VanBlog.Infrastructure.Repositories;

namespace VanBlog.Infrastructure.SeedWorks
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly VanBlogDbContext _context;

        public IPostRepository PostRepository { get; private set; }

        public UnitOfWork(VanBlogDbContext context, IMapper mapper)
        {
            _context = context;
            PostRepository = new PostRepository(_context, mapper);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
