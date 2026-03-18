using BusinessLayer.Entities.Shops;

namespace BusinessLayer.Interfaces.Services
{
    public interface IShopService
    {
        Task<IEnumerable<Shop>> GetAllAsync();

        Task<Shop> GetByIdAsync(int id);

        Task CreateAsync(Shop shop);
    }
}