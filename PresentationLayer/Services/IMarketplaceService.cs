using BusinessLayer.Enums;
using Microsoft.AspNetCore.Http;
using PresentationLayer.ViewModels;

namespace PresentationLayer.Services;

public interface IMarketplaceService
{
    Task<HomePageViewModel> GetHomePageAsync(int? userId, string? searchQuery);

    Task<ProductListPageViewModel> GetProductListPageAsync(
        int? userId,
        string? searchQuery,
        string? category,
        decimal? maxPrice,
        int? minRating,
        string? sort,
        string? genderTag,
        string? sizeTag);

    Task<ProductDetailsPageViewModel?> GetProductDetailsPageAsync(int? userId, int productId);

    Task<(bool Success, string? Error)> AddReviewAsync(int userId, int productId, int rating, string comment);

    Task ToggleFavouriteAsync(int userId, int productId);

    Task AddToCartAsync(int userId, int productId, int quantity = 1);

    Task<CartPageViewModel> GetCartPageAsync(int userId);

    Task<int> GetCartItemsCountAsync(int userId);

    Task SetCartItemQuantityAsync(int userId, int productId, int quantity);

    Task RemoveFromCartAsync(int userId, int productId);

    Task<CheckoutPageViewModel> GetCheckoutPageAsync(int userId, CheckoutInputViewModel? input = null);

    Task<(bool Success, string? Error)> CheckoutAsync(int userId, CheckoutInputViewModel input);

    Task<ProfilePageViewModel?> GetProfilePageAsync(int userId, string? activeTab);

    Task<(bool Success, string? Error)> AddProductForUserAsync(int userId, ProfileCreateProductViewModel model, IReadOnlyList<IFormFile>? imageFiles);

    Task<ProfileEditProductViewModel?> GetProfileEditProductAsync(int userId, int productId);

    Task<(bool Success, string? Error)> UpdateProductByOwnerAsync(int userId, ProfileEditProductViewModel model, IReadOnlyList<IFormFile>? imageFiles);

    Task<(bool Success, string? Error)> DeleteProductForUserAsync(int userId, int productId);

    Task<FavoritesPageViewModel> GetFavoritesPageAsync(int userId);

    Task<IReadOnlyList<string>> GetNavigationCategoriesAsync();

    Task<AdminDashboardViewModel> GetAdminDashboardAsync();

    Task<AdminUserDetailsViewModel?> GetAdminUserDetailsAsync(int userId);

    Task<AdminEditProductViewModel?> GetAdminEditProductAsync(int productId);

    Task<(bool Success, string? Error)> UpdateProductByAdminAsync(AdminEditProductViewModel model);

    Task BanUserAsync(int userId, DateTime bannedUntilUtc, string? reason);

    Task UnbanUserAsync(int userId);

    Task ResolveReportAsync(int reportId, string action);

    Task RejectReportAsync(int reportId, string? reason);

    Task UpdateSystemSettingsAsync(AdminSettingsInputViewModel model);

    Task DeleteProductAsync(int productId);

    Task DeleteReviewAsync(int reviewId);

    Task DeleteUserAsync(int userId);

    Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status);

    Task ResetMarketplaceDataKeepAdminAsync();
}
