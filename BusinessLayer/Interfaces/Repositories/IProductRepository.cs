using BusinessLayer.Entities.Catalog;

namespace BusinessLayer.Interfaces.Repositories
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetByShopIdAsync(int shopId);
    }
}