using BusinessLayer.Common;
using BusinessLayer.Enums;

namespace BusinessLayer.Entities.Orders
{
    public class Payment : BaseEntity
    {
        public int OrderId { get; set; }

        public PaymentStatus Status { get; set; }

        public Order Order { get; set; }
    }
}