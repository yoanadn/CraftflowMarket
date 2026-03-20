using BusinessLayer.Entities.Reviews;
using BusinessLayer.Interfaces.Services;
using DataLayer.Repositories;

namespace ServiceLayer
{
    public class ReviewService : IReviewService
    {
        private readonly UnitOfWork unitOfWork;

        public ReviewService(UnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task AddReviewAsync(Review review)
        {
            await unitOfWork.Reviews.AddAsync(review);
            await unitOfWork.SaveAsync();
        }

        public async Task<IEnumerable<Review>> GetProductReviewsAsync(int productId)
        {
            return await unitOfWork.Reviews.FindAsync(r => r.ProductId == productId);
        }
    }
}