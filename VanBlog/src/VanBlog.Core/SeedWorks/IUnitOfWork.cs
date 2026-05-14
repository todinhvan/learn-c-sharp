using VanBlog.Core.Repositories;

namespace VanBlog.Core.SeedWorks
{
    public interface IUnitOfWork
    {
        IPostRepository PostRepository { get; }
        Task<int> CompleteAsync();
    }
}
