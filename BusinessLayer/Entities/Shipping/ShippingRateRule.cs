using BusinessLayer.Common;

namespace BusinessLayer.Entities.Shipping
{
    public class ShippingRateRule : BaseEntity
    {
        public int ShippingOptionId { get; set; }

        public decimal MinAmount { get; set; }

        public decimal MaxAmount { get; set; }
    }
}