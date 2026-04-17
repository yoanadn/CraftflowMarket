using PresentationLayer.ViewModels.Shared;

namespace PresentationLayer.ViewModels;

public class ProfilePageViewModel
{
    public string FullName { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? ProfileImageUrl { get; set; }

    public string AvatarInitials { get; set; } = "??";

    public DateTime JoinedOn { get; set; }

    public string Bio { get; set; } = string.Empty;

    public IReadOnlyList<ProfileOrderViewModel> Orders { get; set; } = [];

    public IReadOnlyList<ProductCardViewModel> FavouriteProducts { get; set; } = [];

    public IReadOnlyList<ProductCardViewModel> MyProducts { get; set; } = [];

    public ProfileCreateProductViewModel NewProduct { get; set; } = new();

    public IReadOnlyList<string> AvailableCategories { get; set; } = [];

    public string ActiveTab { get; set; } = "orders";
}
