using BusinessLayer.Entities.Shops;
using BusinessLayer.Interfaces.Services;
using DataLayer.Repositories;

namespace ServiceLayer
{
    public class ShopService : IShopService
    {
        private readonly UnitOfWork unitOfWork;

        public ShopService(UnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Shop>> GetAllAsync()
        {
            return await unitOfWork.Shops.GetAllAsync();
        }

        public async Task<Shop> GetByIdAsync(int id)
        {
            return await unitOfWork.Shops.GetByIdAsync(id);
        }

        public async Task CreateAsync(Shop shop)
        {
            await unitOfWork.Shops.AddAsync(shop);
            await unitOfWork.SaveAsync();
        }
    }
}