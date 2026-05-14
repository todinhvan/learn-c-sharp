using TodoList.Core.Entities;
using TodoList.Core.SeedWorks;

namespace TodoList.Core.Repositories
{
    public interface IItemRepository : IRepository<Item, Guid>
    {
    }
}
