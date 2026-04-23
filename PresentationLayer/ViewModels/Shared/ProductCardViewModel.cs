namespace PresentationLayer.ViewModels.Shared;

public class ProductCardViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string? GenderTag { get; set; }

    public string? ColorTag { get; set; }

    public IReadOnlyList<string> SizeTags { get; set; } = [];

    public double Rating { get; set; }

    public int ReviewsCount { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public bool IsFavourite { get; set; }

    public string ShopName { get; set; } = string.Empty;
}
