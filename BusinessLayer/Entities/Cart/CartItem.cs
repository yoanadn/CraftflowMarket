using BusinessLayer.Common;
using BusinessLayer.Entities.Catalog;

namespace BusinessLayer.Entities.Cart
{
    public class CartItem : BaseEntity
    {
        public int CartId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public Cart Cart { get; set; }

        public Product Product { get; set; }
    }
}