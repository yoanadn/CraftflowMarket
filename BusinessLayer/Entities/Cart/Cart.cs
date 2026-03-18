using BusinessLayer.Common;
using BusinessLayer.Entities.Identity;

namespace BusinessLayer.Entities.Cart
{
    public class Cart : BaseEntity
    {
        public int UserId { get; set; }

        public ApplicationUser User { get; set; }

        public ICollection<CartItem> Items { get; set; }
    }
}