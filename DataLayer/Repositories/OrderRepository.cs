using BusinessLayer.Entities.Orders;
using BusinessLayer.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(CraftflowDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
        {
            return await dbSet
                .Where(o => o.UserId == userId)
                .ToListAsync();
        }
    }
}