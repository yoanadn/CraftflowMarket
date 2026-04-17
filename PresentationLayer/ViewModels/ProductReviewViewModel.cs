namespace PresentationLayer.ViewModels;

public class ProductReviewViewModel
{
    public string AuthorName { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; }
}
