using BusinessLayer.Entities.Catalog;
using BusinessLayer.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(CraftflowDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<Product>> GetByShopIdAsync(int shopId)
        {
            return await dbSet
                .Where(p => p.ShopId == shopId)
                .ToListAsync();
        }
    }
}