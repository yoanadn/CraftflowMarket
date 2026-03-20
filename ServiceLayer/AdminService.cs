using BusinessLayer.Interfaces.Services;
using DataLayer.Repositories;

namespace ServiceLayer
{
    public class AdminService : IAdminService
    {
        private readonly UnitOfWork unitOfWork;

        public AdminService(UnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task DeleteProductAsync(int productId)
        {
            var product = await unitOfWork.Products.GetByIdAsync(productId);

            if (product != null)
            {
                unitOfWork.Products.Delete(product);
                await unitOfWork.SaveAsync();
            }
        }

        public async Task BanUserAsync(int userId)
        {
        }
    }
}