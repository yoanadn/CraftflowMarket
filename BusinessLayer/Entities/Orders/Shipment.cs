using BusinessLayer.Common;
using BusinessLayer.Enums;

namespace BusinessLayer.Entities.Orders
{
    public class Shipment : BaseEntity
    {
        public int OrderId { get; set; }

        public ShipmentStatus Status { get; set; }

        public string Address { get; set; }

        public Order Order { get; set; }
    }
}