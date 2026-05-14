using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VanBlog.Core.SeedWorks;
using VanBlog.Infrastructure.Data;

namespace VanBlog.Infrastructure.SeedWorks
{
    public class Repository<TClass, Key> : IRepository<TClass, Key> where TClass : class
    {
        protected readonly VanBlogDbContext _context;
        private readonly DbSet<TClass> _dbSet;

        public Repository(VanBlogDbContext context)
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

        public async Task<IEnumerable<TClass>> GetAsync(Expression<Func<TClass, bool>> expression)
        {
            return await _dbSet.Where(expression).ToListAsync();
        }

        public async Task<TClass?> GetByIdAsync(Key id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async void Remove(TClass entity)
        {
            _dbSet.Remove(entity);
        }

        public async void RemoveRange(IEnumerable<TClass> entities)
        {
            _dbSet.RemoveRange(entities);
        }
    }
}
