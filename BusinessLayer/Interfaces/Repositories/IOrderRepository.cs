using BusinessLayer.Entities.Orders;

namespace BusinessLayer.Interfaces.Repositories
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
    }
}