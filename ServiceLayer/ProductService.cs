using BusinessLayer.Entities.Catalog;
using BusinessLayer.Interfaces.Services;
using DataLayer.Repositories;

namespace ServiceLayer
{
    public class ProductService : IProductService
    {
        private readonly UnitOfWork unitOfWork;

        public ProductService(UnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await unitOfWork.Products.GetAllAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await unitOfWork.Products.GetByIdAsync(id);
        }

        public async Task CreateAsync(Product product)
        {
            await unitOfWork.Products.AddAsync(product);
            await unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            unitOfWork.Products.Update(product);
            await unitOfWork.SaveAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await unitOfWork.Products.GetByIdAsync(id);

            if (product != null)
            {
                unitOfWork.Products.Delete(product);
                await unitOfWork.SaveAsync();
            }
        }
    }
}