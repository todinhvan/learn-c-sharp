using GHTK.Infrastructure.Entities;

namespace GHTK.Infrastructure.Repositories
{
    public interface IOrderRepository
    {
        Task CreateOrderAsync(Order order);
        Task<Order?> FindOrderAsync(string id, string partnerId);
        Task<bool> CancelOrderAsync(string trackingId, string partnerId);
    }
}
