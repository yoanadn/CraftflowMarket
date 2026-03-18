using BusinessLayer.Common;
using BusinessLayer.Entities.Identity;
using BusinessLayer.Entities.Catalog;

namespace BusinessLayer.Entities.Social
{
    public class FavouriteProduct : BaseEntity
    {
        public int UserId { get; set; }

        public int ProductId { get; set; }

        public ApplicationUser User { get; set; }

        public Product Product { get; set; }
    }
}