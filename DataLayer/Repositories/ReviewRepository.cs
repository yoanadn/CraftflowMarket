using BusinessLayer.Entities.Reviews;
using BusinessLayer.Interfaces.Repositories;

namespace DataLayer.Repositories
{
    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        public ReviewRepository(CraftflowDbContext context) : base(context)
        {
        }
    }
}