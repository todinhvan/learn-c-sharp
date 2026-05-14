using CoreConcept.AuthDemo.Entities;

namespace CoreConcept.AuthDemo.Repositories
{
    public interface IUserRepository
    {
        Task<User?> FindByUserNameAsync(string userName);
        Task SaveAsync(User user);
    }
}
