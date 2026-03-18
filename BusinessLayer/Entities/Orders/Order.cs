using BusinessLayer.Common;
using BusinessLayer.Entities.Identity;
using BusinessLayer.Enums;

namespace BusinessLayer.Entities.Orders
{
    public class Order : BaseEntity
    {
        public int UserId { get; set; }

        public OrderStatus Status { get; set; }

        public ApplicationUser User { get; set; }

        public ICollection<OrderItem> Items { get; set; }
    }
}