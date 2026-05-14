using Microsoft.EntityFrameworkCore;
using TodoList.Core.SeedWorks;
using TodoList.Infrastructure.Data;

namespace TodoList.Infrastructure.SeedWorks
{
    public class Repository<TClass, Key> : IRepository<TClass, Key> where TClass : class
    {
        protected readonly TodoListDbContext _context;
        private readonly DbSet<TClass> _dbSet;

        public Repository(TodoListDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TClass>();
        }

        public void Add(TClass entity)
        {
            _dbSet.Add(entity);
        }

        public void AddRange(IEnumerable<TClass> entities)
        {
            _dbSet.AddRange(entities);
        }

        public async Task<IEnumerable<TClass>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<TClass?> GetByIdAsync(Key id)
        {
            return await _dbSet.FindAsync(id);
        }

        public void Remove(TClass entity)
        {
            _dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<TClass> entities)
        {
            _dbSet.RemoveRange(entities);
        }
    }
}
