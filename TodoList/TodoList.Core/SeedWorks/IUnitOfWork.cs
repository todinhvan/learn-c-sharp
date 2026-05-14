using TodoList.Core.Repositories;

namespace TodoList.Core.SeedWorks
{
    public interface IUnitOfWork
    {
        IItemRepository ItemRepository { get; init; }

        Task<int> CompleteAsync();
    }
}
