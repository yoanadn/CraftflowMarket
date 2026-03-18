using BusinessLayer.Entities.Catalog;

namespace BusinessLayer.Interfaces.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllAsync();

        Task<Product> GetByIdAsync(int id);

        Task CreateAsync(Product product);

        Task UpdateAsync(Product product);

        Task DeleteAsync(int id);
    }
}