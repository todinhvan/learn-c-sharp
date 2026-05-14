using TodoList.Core.Repositories;
using TodoList.Core.SeedWorks;
using TodoList.Infrastructure.Data;
using TodoList.Infrastructure.Repositories;

namespace TodoList.Infrastructure.SeedWorks
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TodoListDbContext _context;
        public IItemRepository ItemRepository { get; init; }

        public UnitOfWork(TodoListDbContext context)
        {
            _context = context;
            ItemRepository = new ItemRepository(_context);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
