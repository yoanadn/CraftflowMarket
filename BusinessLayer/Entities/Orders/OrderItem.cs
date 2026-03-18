using BusinessLayer.Common;
using BusinessLayer.Entities.Catalog;

namespace BusinessLayer.Entities.Orders
{
    public class OrderItem : BaseEntity
    {
        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public Product Product { get; set; }

        public Order Order { get; set; }
    }
}