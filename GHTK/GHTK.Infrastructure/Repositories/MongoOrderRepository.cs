using GHTK.Infrastructure.Entities;
using MongoDB.Driver;

namespace GHTK.Infrastructure.Repositories
{
    public class MongoOrderRepository : IOrderRepository
    {
        private readonly IMongoCollection<Order> _orderCollection;

        public MongoOrderRepository(MongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("GhtkDB");
            _orderCollection = database.GetCollection<Order>("orders");
        }
        public async Task<bool> CancelOrderAsync(string trackingId, string partnerId)
        {
            var filter = Builders<Order>.Filter.Eq(o => o.TrackingId, trackingId)
                            & Builders<Order>.Filter.Eq(o => o.PartnerId, partnerId);
            var update = Builders<Order>.Update.Set(o => o.Status, -1);
            var result = await _orderCollection.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0;
        }

        public async Task CreateOrderAsync(Order order)
        {
            await _orderCollection.InsertOneAsync(order);
        }

        public async Task<Order?> FindOrderAsync(string id, string partnerId)
        {
            var filter = Builders<Order>.Filter.Eq(o => o.Id, id)
                            & Builders<Order>.Filter.Eq(o => o.PartnerId, partnerId);
            return await _orderCollection.Find(filter).FirstOrDefaultAsync();
        }
    }
}
