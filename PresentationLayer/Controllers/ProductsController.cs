using Microsoft.AspNetCore.Mvc;
using PresentationLayer.Infrastructure;
using PresentationLayer.Services;

namespace PresentationLayer.Controllers;

[Route("products")]
public class ProductsController : Controller
{
    private readonly IMarketplaceService marketplaceService;
    private readonly IUserSessionService userSession;

    public ProductsController(IMarketplaceService marketplaceService, IUserSessionService userSession)
    {
        this.marketplaceService = marketplaceService;
        this.userSession = userSession;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        string? search,
        string? category,
        decimal? maxPrice,
        int? minRating,
        string? sort,
        string? genderTag,
        string? sizeTag)
    {
        var model = await marketplaceService.GetProductListPageAsync(
            userSession.UserId,
            search,
            category,
            maxPrice,
            minRating,
            sort,
            genderTag,
            sizeTag);

        return View(model);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var model = await marketplaceService.GetProductDetailsPageAsync(userSession.UserId, id);
        return model is null ? NotFound() : View(model);
    }

    [ValidateAntiForgeryToken]
    [HttpPost("{id:int}/review")]
    public async Task<IActionResult> AddReview(int id, int rating, string comment, string? returnUrl = null)
    {
        if (userSession.IsAdmin)
        {
            TempData["Error"] = "Админ акаунтите не могат да публикуват отзиви.";
            return RedirectToAction("Index", "Admin");
        }

        if (!userSession.IsAuthenticated || userSession.UserId is null)
        {
            TempData["Error"] = "Моля, влез в профила си, за да оставиш отзив.";
            return RedirectToAction("Login", "Account", new { returnUrl = returnUrl ?? Url.Action(nameof(Details), new { id }) });
        }

        var result = await marketplaceService.AddReviewAsync(userSession.UserId.Value, id, rating, comment);
        TempData[result.Success ? "Success" : "Error"] = result.Success
            ? "Отзивът е изпратен успешно."
            : result.Error ?? "Неуспешно изпращане на отзив.";

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [ValidateAntiForgeryToken]
    [HttpPost("favourite")]
    public async Task<IActionResult> ToggleFavourite(int productId, string? returnUrl)
    {
        if (userSession.IsAdmin)
        {
            TempData["Error"] = "Админ акаунтите не използват любими. Използвай админ панела за модерация.";
            return RedirectToAction("Index", "Admin");
        }

        if (!userSession.IsAuthenticated || userSession.UserId is null)
        {
            TempData["Error"] = "Моля, влез в профила си, за да добавяш продукти в любими.";
            return RedirectToAction("Login", "Account", new { returnUrl = returnUrl ?? Url.Action(nameof(Index)) });
        }

        await marketplaceService.ToggleFavouriteAsync(userSession.UserId.Value, productId);
        return Redirect(returnUrl ?? Url.Action(nameof(Index))!);
    }

    [ValidateAntiForgeryToken]
    [HttpPost("add-to-cart")]
    public async Task<IActionResult> AddToCart(int productId, int quantity = 1, string? returnUrl = null)
    {
        if (userSession.IsAdmin)
        {
            TempData["Error"] = "Админ акаунтите не могат да правят поръчки.";
            return RedirectToAction("Index", "Admin");
        }

        if (!userSession.IsAuthenticated || userSession.UserId is null)
        {
            TempData["Error"] = "Моля, влез в профила си, преди да добавяш продукти в количката.";
            return RedirectToAction("Login", "Account", new { returnUrl = returnUrl ?? Url.Action("Index", "Cart") });
        }

        await marketplaceService.AddToCartAsync(userSession.UserId.Value, productId, quantity);
        TempData["Success"] = "Продуктът е добавен в количката.";
        return Redirect(returnUrl ?? Url.Action("Index", "Cart")!);
    }
}
