using BusinessLayer.Entities.Reviews;

namespace BusinessLayer.Interfaces.Services
{
    public interface IReviewService
    {
        Task AddReviewAsync(Review review);

        Task<IEnumerable<Review>> GetProductReviewsAsync(int productId);
    }
}