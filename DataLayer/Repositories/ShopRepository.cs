using BusinessLayer.Entities.Shops;
using BusinessLayer.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Repositories
{
    public class ShopRepository : GenericRepository<Shop>, IShopRepository
    {
        public ShopRepository(CraftflowDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<Shop>> GetByOwnerIdAsync(int ownerId)
        {
            return await dbSet
                .Where(s => s.OwnerId == ownerId)
                .ToListAsync();
        }
    }
}