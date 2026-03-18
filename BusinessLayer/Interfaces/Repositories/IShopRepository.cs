using BusinessLayer.Entities.Shops;

namespace BusinessLayer.Interfaces.Repositories
{
    public interface IShopRepository : IGenericRepository<Shop>
    {
        Task<IEnumerable<Shop>> GetByOwnerIdAsync(int ownerId);
    }
}