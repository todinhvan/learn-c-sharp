namespace TodoList.Core.SeedWorks
{
    public interface IRepository<TClass, Key> where TClass : class
    {
        Task<TClass?> GetByIdAsync(Key id);
        Task<IEnumerable<TClass>> GetAllAsync();
        void Add(TClass entity);
        void AddRange(IEnumerable<TClass> entities);
        void Remove(TClass entity);
        void RemoveRange(IEnumerable<TClass> entities);
    }
}
