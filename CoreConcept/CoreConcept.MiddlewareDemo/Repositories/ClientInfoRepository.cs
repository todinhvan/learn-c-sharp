using CoreConcept.MiddlewareDemo.Models;

namespace CoreConcept.MiddlewareDemo.Repositories
{
    public class ClientInfoRepository : IClientInfoRepository
    {
        private readonly List<ClientInfo> _clientInfos;

        public ClientInfoRepository()
        {
            _clientInfos = new List<ClientInfo>
            {
                new ClientInfo { Id = 1, Name = "Client A" },
                new ClientInfo { Id = 2, Name = "Client B" },
                new ClientInfo { Id = 3, Name = "Client C" }
            };
        }
        public ClientInfo? GetClientInfo(int id)
        {
            return _clientInfos.FirstOrDefault(c => c.Id == id);
        }
    }
}
