using System.Linq.Expressions;

namespace VanBlog.Core.SeedWorks
{
    public interface IRepository<TClass, Key> where TClass : class
    {
        Task<IEnumerable<TClass>> GetAllAsync();
        Task<IEnumerable<TClass>> GetAsync(Expression<Func<TClass, bool>> expression);
        Task<TClass?> GetByIdAsync(Key id); 
        void Add(TClass entity);
        void AddRange(IEnumerable<TClass> entities);
        void Remove(TClass entity);
        void RemoveRange(IEnumerable<TClass> entities);
    }
}
