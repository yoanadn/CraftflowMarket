using BusinessLayer.Enums;

namespace PresentationLayer.ViewModels;

public class ProfileOrderViewModel
{
    public int OrderId { get; set; }

    public DateTime CreatedOn { get; set; }

    public OrderStatus Status { get; set; }

    public decimal Total { get; set; }

    public IReadOnlyList<ProfileOrderItemViewModel> Items { get; set; } = [];
}
