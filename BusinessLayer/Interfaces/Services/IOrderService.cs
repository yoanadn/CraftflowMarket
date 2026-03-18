using BusinessLayer.Entities.Orders;

namespace BusinessLayer.Interfaces.Services
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(int userId);

        Task<IEnumerable<Order>> GetUserOrdersAsync(int userId);
    }
}