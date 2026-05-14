using OpenIDConnect.Server.Models;

namespace OpenIDConnect.Server.Repositories
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = [
            new() { Name = "van"},
            new() {Name = "dinh"}
        ];

        public User? FindByUserName(string userName)
        {
            return _users.FirstOrDefault(u => u.Name.Equals(userName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
