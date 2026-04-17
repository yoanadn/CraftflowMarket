using BusinessLayer.Enums;
using System.ComponentModel.DataAnnotations;
using PresentationLayer.ViewModels.Shared;

namespace PresentationLayer.ViewModels;

public class AdminDashboardViewModel
{
    public IReadOnlyList<AdminUserViewModel> Users { get; set; } = [];

    public IReadOnlyList<AdminProductManageViewModel> Products { get; set; } = [];

    public IReadOnlyList<AdminReviewViewModel> Reviews { get; set; } = [];

    public IReadOnlyList<AdminReportViewModel> Reports { get; set; } = [];

    public IReadOnlyList<AdminModerationActionViewModel> Actions { get; set; } = [];

    public IReadOnlyList<AdminOrderViewModel> Orders { get; set; } = [];

    public IReadOnlyList<string> Categories { get; set; } = [];

    public int TotalUsers { get; set; }

    public int TotalProducts { get; set; }

    public int TotalOrders { get; set; }

    public int TotalReviews { get; set; }

    public decimal EstimatedRevenue { get; set; }
}

public class AdminUserViewModel
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; }

    public bool IsBanned { get; set; }

    public DateTime? BannedUntil { get; set; }
}

public class AdminReviewViewModel
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; }
}

public class AdminProductManageViewModel : ProductCardViewModel
{
    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}

public class AdminReportViewModel
{
    public int Id { get; set; }

    public ReportTargetType TargetType { get; set; }

    public int TargetId { get; set; }

    public ReportStatus Status { get; set; }

    public string TargetSummary { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; }
}

public class AdminModerationActionViewModel
{
    public int Id { get; set; }

    public int ReportId { get; set; }

    public string Action { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; }
}

public class AdminEditProductViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Полето е задължително.")]
    [MinLength(3, ErrorMessage = "Името трябва да е поне 3 символа.")]
    [Display(Name = "Име на продукта")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Полето е задължително.")]
    [Display(Name = "Категория")]
    public string Category { get; set; } = string.Empty;

    [Range(1, 100000, ErrorMessage = "Цената трябва да е между 1 и 100000.")]
    [Display(Name = "Цена")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Полето е задължително.")]
    [MinLength(10, ErrorMessage = "Описанието трябва да е поне 10 символа.")]
    [Display(Name = "Описание")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Полето е задължително.")]
    [Display(Name = "Статус")]
    public ProductStatus Status { get; set; }
}

public class AdminUserDetailsViewModel
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string StoredPassword { get; set; } = string.Empty;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? ProfileImageUrl { get; set; }

    public string AvatarInitials { get; set; } = "??";

    public DateTime CreatedOn { get; set; }

    public bool IsBanned { get; set; }

    public DateTime? BannedUntil { get; set; }

    public int OrdersCount { get; set; }

    public int ReviewsCount { get; set; }

    public int ProductsCount { get; set; }

    public IReadOnlyList<AdminUserOrderDetailsViewModel> Orders { get; set; } = [];

    public IReadOnlyList<AdminUserReviewDetailsViewModel> Reviews { get; set; } = [];

    public IReadOnlyList<AdminUserProductDetailsViewModel> Products { get; set; } = [];
}

public class AdminUserOrderDetailsViewModel
{
    public int Id { get; set; }

    public DateTime CreatedOn { get; set; }

    public OrderStatus Status { get; set; }

    public string AddressSummary { get; set; } = string.Empty;

    public string PaymentMethod { get; set; } = string.Empty;

    public decimal Subtotal { get; set; }

    public decimal ShippingAmount { get; set; }

    public decimal Total => Subtotal + ShippingAmount;

    public int ItemsCount { get; set; }

    public string ItemsSummary { get; set; } = string.Empty;
}

public class AdminUserReviewDetailsViewModel
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; }
}

public class AdminUserProductDetailsViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; }
}

public class AdminSettingsInputViewModel
{
    [Required(ErrorMessage = "Полето е задължително.")]
    [Display(Name = "Категории")]
    public string CategoriesRaw { get; set; } = string.Empty;
}

public class AdminOrderViewModel
{
    public int Id { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; }

    public OrderStatus Status { get; set; }

    public string AddressSummary { get; set; } = string.Empty;

    public string PaymentMethod { get; set; } = string.Empty;

    public decimal Subtotal { get; set; }

    public decimal ShippingAmount { get; set; }

    public decimal Total => Subtotal + ShippingAmount;

    public int ItemsCount { get; set; }
}

