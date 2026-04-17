namespace PresentationLayer.ViewModels;

public class CartPageViewModel
{
    public IReadOnlyList<CartItemViewModel> Items { get; set; } = [];

    public decimal Subtotal { get; set; }

    public decimal Shipping { get; set; }

    public bool RequiresLogin { get; set; }

    public string? InfoMessage { get; set; }

    public decimal Total => Subtotal + Shipping;
}
