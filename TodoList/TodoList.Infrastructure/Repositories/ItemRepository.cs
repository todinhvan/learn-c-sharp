using TodoList.Core.Entities;
using TodoList.Core.Repositories;
using TodoList.Infrastructure.Data;
using TodoList.Infrastructure.SeedWorks;

namespace TodoList.Infrastructure.Repositories
{
    public class ItemRepository : Repository<Item, Guid>, IItemRepository
    {
        public ItemRepository(TodoListDbContext context) : base(context)
        {
        }
    }
}
