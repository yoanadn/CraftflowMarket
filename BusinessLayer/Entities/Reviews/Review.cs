using BusinessLayer.Common;
using BusinessLayer.Entities.Identity;
using BusinessLayer.Entities.Catalog;

namespace BusinessLayer.Entities.Reviews
{
    public class Review : BaseEntity
    {
        public int ProductId { get; set; }

        public int UserId { get; set; }

        public int Rating { get; set; }

        public string Comment { get; set; }

        public Product Product { get; set; }

        public ApplicationUser User { get; set; }
    }
}