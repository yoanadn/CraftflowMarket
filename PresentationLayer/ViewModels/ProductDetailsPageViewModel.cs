using PresentationLayer.ViewModels.Shared;

namespace PresentationLayer.ViewModels;

public class ProductDetailsPageViewModel
{
    public int ProductId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string? GenderTag { get; set; }

    public IReadOnlyList<string> SizeTags { get; set; } = [];

    public double Rating { get; set; }

    public int ReviewsCount { get; set; }

    public bool IsFavourite { get; set; }

    public string ShopName { get; set; } = string.Empty;

    public string ShopDescription { get; set; } = string.Empty;

    public IReadOnlyList<string> ImageUrls { get; set; } = [];

    public IReadOnlyList<ProductReviewViewModel> Reviews { get; set; } = [];

    public IReadOnlyList<ProductCardViewModel> RelatedProducts { get; set; } = [];
}
