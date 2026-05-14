using OpenIDConnect.Server.Models;

namespace OpenIDConnect.Server.Repositories
{
    public interface IUserRepository
    {
        User? FindByUserName(string userName);
    }
}
