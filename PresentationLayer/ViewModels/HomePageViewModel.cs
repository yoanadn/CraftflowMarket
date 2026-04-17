using PresentationLayer.ViewModels.Shared;

namespace PresentationLayer.ViewModels;

public class HomePageViewModel
{
    public string SearchQuery { get; set; } = string.Empty;

    public IReadOnlyList<ProductCardViewModel> FeaturedProducts { get; set; } = [];

    public IReadOnlyList<ProductCardViewModel> FavouriteProducts { get; set; } = [];

    public IReadOnlyList<ProductCardViewModel> NewestProducts { get; set; } = [];

    public IReadOnlyList<CategoryTileViewModel> PopularCategories { get; set; } = [];
}
