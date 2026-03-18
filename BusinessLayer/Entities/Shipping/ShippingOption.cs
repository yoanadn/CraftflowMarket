using BusinessLayer.Common;

namespace BusinessLayer.Entities.Shipping
{
    public class ShippingOption : BaseEntity
    {
        public string Name { get; set; }

        public decimal Price { get; set; }
    }
}