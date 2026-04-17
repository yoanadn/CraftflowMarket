using PresentationLayer.ViewModels.Shared;

namespace PresentationLayer.ViewModels;

public class FavoritesPageViewModel
{
    public IReadOnlyList<ProductCardViewModel> Products { get; set; } = [];
}
