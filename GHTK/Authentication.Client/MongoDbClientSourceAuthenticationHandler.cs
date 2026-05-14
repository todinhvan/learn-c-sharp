using MongoDB.Driver;

namespace Authentication.Client
{
    public class MongoDbClientSourceAuthenticationHandler : IClientSourceAuthenticationHandler, IDisposable
    {
        private readonly MongoClient _client;
        private readonly IMongoCollection<ClientSource> _clientSourceCollection;
        private bool disposedValue = false;

        public MongoDbClientSourceAuthenticationHandler(string connectString)
        {
            _client = new MongoClient(connectString);
            IMongoDatabase database = _client.GetDatabase("GhtkDB");
            _clientSourceCollection = database.GetCollection<ClientSource>("client_sources");
        }
        public async Task<bool> AuthenticateAsync(string clientSource)
        {
            var filter = Builders<ClientSource>.Filter.Eq(c => c.ClientId, clientSource)
                    & Builders<ClientSource>.Filter.Lte(c => c.ValidFrom, DateTime.UtcNow)
                    & Builders<ClientSource>.Filter.Gte(c => c.ValidTo, DateTime.UtcNow)
                    & Builders<ClientSource>.Filter.Eq(c => c.IsEnable, true);

            var result = await _clientSourceCollection.FindAsync(filter);
            return await result.AnyAsync();
        }

        public void Dispose()
        {
            if (!disposedValue)
            {
                _client.Dispose();
            }
            disposedValue = true;
        }
    }

    public class ClientSource
    {
        public string ClientId { get; set; } = default!;
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public bool IsEnable { get; set; }
    }
}
