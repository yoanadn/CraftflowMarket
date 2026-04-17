using BusinessLayer.Common;
using BusinessLayer.Entities.Identity;
using BusinessLayer.Enums;

namespace BusinessLayer.Entities.Orders
{
    public class Order : BaseEntity
    {
        public int UserId { get; set; }

        public OrderStatus Status { get; set; }
        
        public string RecipientName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string StreetAddress { get; set; } = string.Empty;

        public string PostalCode { get; set; } = string.Empty;

        public string PaymentMethod { get; set; } = string.Empty;

        public decimal ShippingAmount { get; set; }

        public ApplicationUser User { get; set; } = null!;

        public ICollection<OrderItem> Items { get; set; } = [];
    }
}
