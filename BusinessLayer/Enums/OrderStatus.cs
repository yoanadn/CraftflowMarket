namespace BusinessLayer.Enums
{
    public enum OrderStatus
    {
        Pending = 0,
        Processing = 1,
        Sent = 2,
        Shipped = Sent,
        Delivered = 3,
        Completed = Delivered,
        Cancelled = 4
    }
}
