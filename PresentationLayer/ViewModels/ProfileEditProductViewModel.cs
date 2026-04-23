using System.ComponentModel.DataAnnotations;

namespace PresentationLayer.ViewModels;

public class ProfileEditProductViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Полето е задължително.")]
    [MinLength(3, ErrorMessage = "Името трябва да е поне 3 символа.")]
    [Display(Name = "Име на продукта")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Полето е задължително.")]
    [Display(Name = "Категория")]
    public string Category { get; set; } = string.Empty;

    [Range(0.01, 100000, ErrorMessage = "Цената трябва да е между 0.01 и 100000.")]
    [Display(Name = "Цена")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Полето е задължително.")]
    [MinLength(10, ErrorMessage = "Описанието трябва да е поне 10 символа.")]
    [Display(Name = "Описание")]
    public string Description { get; set; } = string.Empty;

    public string? GenderTag { get; set; }

    public string? ColorTag { get; set; }

    public string? SizeTags { get; set; }

    public IReadOnlyList<ProfileProductImageViewModel> ExistingImages { get; set; } = [];

    public List<int> RemoveImageIds { get; set; } = [];

    public IReadOnlyList<string> AvailableCategories { get; set; } = [];
}

public class ProfileProductImageViewModel
{
    public int Id { get; set; }

    public string Url { get; set; } = string.Empty;
}
