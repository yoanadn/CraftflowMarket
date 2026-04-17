using PresentationLayer.ViewModels.Shared;

namespace PresentationLayer.ViewModels;

public class ProductListPageViewModel
{
    public string SearchQuery { get; set; } = string.Empty;

    public string Category { get; set; } = "All";

    public decimal MaxPrice { get; set; } = 250;

    public decimal MaxSelectablePrice { get; set; } = 250;

    public int MinRating { get; set; }

    public string Sort { get; set; } = "featured";

    public string GenderTag { get; set; } = string.Empty;

    public string SizeTag { get; set; } = string.Empty;

    public int ProductCount { get; set; }

    public IReadOnlyList<string> Categories { get; set; } = [];

    public IReadOnlyList<string> GenderTags { get; set; } = [];

    public IReadOnlyList<string> SizeTags { get; set; } = [];

    public IReadOnlyList<ProductCardViewModel> Products { get; set; } = [];
}
