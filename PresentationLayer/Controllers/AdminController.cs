using Microsoft.AspNetCore.Mvc;
using BusinessLayer.Enums;
using PresentationLayer.Infrastructure;
using PresentationLayer.Services;
using PresentationLayer.ViewModels;

namespace PresentationLayer.Controllers;

[Route("admin")]
public class AdminController : Controller
{
    private readonly IMarketplaceService marketplaceService;
    private readonly IUserSessionService userSession;

    public AdminController(IMarketplaceService marketplaceService, IUserSessionService userSession)
    {
        this.marketplaceService = marketplaceService;
        this.userSession = userSession;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var gate = EnsureAdminAccess();
        if (gate is not null)
        {
            return gate;
        }

        var model = await marketplaceService.GetAdminDashboardAsync();
        return View(model);
    }

    [HttpGet("users/{userId:int}")]
    public async Task<IActionResult> UserDetails(int userId)
    {
        var gate = EnsureAdminAccess();
        if (gate is not null)
        {
            return gate;
        }

        var model = await marketplaceService.GetAdminUserDetailsAsync(userId);
        return model is null ? NotFound() : View(model);
    }

    [ValidateAntiForgeryToken]
    [HttpPost("users/ban")]
    public async Task<IActionResult> BanUser(int userId, int banDays = 7, string? reason = null)
    {
        var gate = EnsureAdminAccess();
        if (gate is not null)
        {
            return gate;
        }

        if (userSession.UserId == userId)
        {
            TempData["Error"] = "Не можеш да блокираш собствения си админ акаунт.";
            return RedirectToAction(nameof(Index));
        }

        if (banDays <= 0)
        {
            banDays = 7;
        }

        await marketplaceService.BanUserAsync(userId, DateTime.UtcNow.AddDays(banDays), reason);
        TempData["Success"] = $"Потребител #{userId} е блокиран за {banDays} дни.";
        return RedirectToAction(nameof(Index));
    }

    [ValidateAntiForgeryToken]
    [HttpPost("users/unban")]
    public async Task<IActionResult> UnbanUser(int userId)
    {
        var gate = EnsureAdminAccess();
        if (gate is not null)
        {
            return gate;
        }

        await marketplaceService.UnbanUserAsync(userId);
        TempData["Success"] = $"Потребител #{userId} е разблокиран.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("products/{productId:int}/edit")]
    public async Task<IActionResult> EditProduct(int productId)
    {
        var gate = EnsureAdminAccess();
        if (gate is not null)
        {
            return gate;
        }

        var model = await marketplaceService.GetAdminEditProductAsync(productId);
        return model is null ? NotFound() : View(model);
    }

    [ValidateAntiForgeryToken]
    [HttpPost("products/{productId:int}/edit")]
    public async Task<IActionResult> EditProduct(int productId, AdminEditProductViewModel model)
    {
        var gate = EnsureAdminAccess();
        if (gate is not null)
        {
            return gate;
        }

        model.Id = productId;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await marketplaceService.UpdateProductByAdminAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Неуспешно обновяване на продукта.");
            return View(model);
        }

        TempData["Success"] = "Продуктът е обновен.";
        return RedirectToAction(nameof(Index));
    }

    [ValidateAntiForgeryToken]
    [HttpPost("orders/status")]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus status)
    {
        var gate = EnsureAdminAccess();
        if (gate is not null)
        {
            return gate;
        }

        var updated = await marketplaceService.UpdateOrderStatusAsync(orderId, status);
        if (!updated)
        {
            TempData["Error"] = $"Поръчка #{orderId} не е намерена.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = $"Статусът на поръчка #{orderId} е обновен.";
        return RedirectToAction(nameof(Index));
    }

    [ValidateAntiForgeryToken]
    [HttpPost("reports/resolve")]
    public async Task<IActionResult> ResolveReport(int reportId, string? action)
    {
        var gate = EnsureAdminAccess();
        if (gate is not null)
        {
            return gate;
        }

        await marketplaceService.ResolveReportAsync(reportId, action ?? string.Empty);
        TempData["Success"] = "Сигналът е маркиран като решен.";
        return RedirectToAction(nameof(Index));
    }

    [ValidateAntiForgeryToken]
    [HttpPost("reports/reject")]
    public async Task<IActionResult> RejectReport(int reportId, string? reason)
    {
        var gate = EnsureAdminAccess();
        if (gate is not null)
        {
            return gate;
        }

        await marketplaceService.RejectReportAsync(reportId, reason);
        TempData["Success"] = "Сигналът е отхвърлен.";
        return RedirectToAction(nameof(Index));
    }

    [ValidateAntiForgeryToken]
    [HttpPost("settings")]
    public async Task<IActionResult> UpdateSettings(AdminSettingsInputViewModel model)
    {
        var gate = EnsureAdminAccess();
        if (gate is not null)
        {
            return gate;
        }

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Моля, попълни всички полета в настройките.";
            return RedirectToAction(nameof(Index));
        }

        await marketplaceService.UpdateSystemSettingsAsync(model);
        TempData["Success"] = "Системните настройки са обновени.";
        return RedirectToAction(nameof(Index));
    }

    [ValidateAntiForgeryToken]
    [HttpPost("data/reset")]
    public async Task<IActionResult> ResetMarketplaceData()
    {
        var gate = EnsureAdminAccess();
        if (gate is not null)
        {
            return gate;
        }

        await marketplaceService.ResetMarketplaceDataKeepAdminAsync();
        TempData["Success"] = "Всички данни бяха изтрити. Запазени са само администраторските акаунти.";
        return RedirectToAction(nameof(Index));
    }

    [ValidateAntiForgeryToken]
    [HttpPost("products/delete")]
    public async Task<IActionResult> DeleteProduct(int productId)
    {
        var gate = EnsureAdminAccess();
        if (gate is not null)
        {
            return gate;
        }

        await marketplaceService.DeleteProductAsync(productId);
        TempData["Success"] = "Продуктът е премахнат.";
        return RedirectToAction(nameof(Index));
    }

    [ValidateAntiForgeryToken]
    [HttpPost("reviews/delete")]
    public async Task<IActionResult> DeleteReview(int reviewId)
    {
        var gate = EnsureAdminAccess();
        if (gate is not null)
        {
            return gate;
        }

        await marketplaceService.DeleteReviewAsync(reviewId);
        TempData["Success"] = "Отзивът е премахнат.";
        return RedirectToAction(nameof(Index));
    }

    [ValidateAntiForgeryToken]
    [HttpPost("users/delete")]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        var gate = EnsureAdminAccess();
        if (gate is not null)
        {
            return gate;
        }

        if (userSession.UserId == userId)
        {
            TempData["Error"] = "Не можеш да изтриеш собствения си админ акаунт.";
            return RedirectToAction(nameof(Index));
        }

        await marketplaceService.DeleteUserAsync(userId);
        TempData["Success"] = "Потребителят е изтрит.";
        return RedirectToAction(nameof(Index));
    }

    private IActionResult? EnsureAdminAccess()
    {
        if (!userSession.IsAuthenticated)
        {
            TempData["Error"] = "Моля, влез в профила си, за да достъпиш админ таблото.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Index)) });
        }

        if (!userSession.IsAdmin)
        {
            TempData["Error"] = "Само админи могат да отворят тази страница.";
            return RedirectToAction("Index", "Home");
        }

        return null;
    }
}
