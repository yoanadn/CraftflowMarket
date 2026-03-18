using BusinessLayer.Common;
using BusinessLayer.Entities.Identity;
using BusinessLayer.Entities.Shops;

namespace BusinessLayer.Entities.Social
{
    public class FavouriteShop : BaseEntity
    {
        public int UserId { get; set; }

        public int ShopId { get; set; }

        public ApplicationUser User { get; set; }

        public Shop Shop { get; set; }
    }
}