using CoreConcept.AuthDemo.Entities;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CoreConcept.AuthDemo.Repositories
{
    public class InJsonUserRepository : IUserRepository
    {
        private readonly string _jsonPath;

        public InJsonUserRepository(IOptions<InJsonUserRepositoryOptions> options)
        {
            _jsonPath = options.Value.JsonDirectoryPath;

            if (!Directory.Exists(_jsonPath)) { 
                Directory.CreateDirectory(_jsonPath);
            }
        }

        public async Task<User?> FindByUserNameAsync(string userName)
        {
            var file = new FileInfo(Path.Combine(_jsonPath, userName));
            if (!file.Exists)
            {
                return null;
            }

            var json = await File.ReadAllTextAsync(file.FullName);

            return JsonSerializer.Deserialize<User>(json);
        }

        public Task SaveAsync(User user)
        {
            var file = new FileInfo(Path.Combine(_jsonPath, user.UserName));
            var json = JsonSerializer.Serialize(user);

            return File.WriteAllTextAsync(file.FullName, json);
        }
    }
}
