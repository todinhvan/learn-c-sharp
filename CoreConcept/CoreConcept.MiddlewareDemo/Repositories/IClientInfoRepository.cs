using CoreConcept.MiddlewareDemo.Models;

namespace CoreConcept.MiddlewareDemo.Repositories
{
    public interface IClientInfoRepository
    {
        ClientInfo? GetClientInfo(int id);
    }
}
