using Microsoft.AspNetCore.Mvc;
using PresentationLayer.Infrastructure;
using PresentationLayer.Services;
using PresentationLayer.ViewModels;

namespace PresentationLayer.Controllers;

[Route("cart")]
public class CartController : Controller
{
    private readonly IMarketplaceService marketplaceService;
    private readonly IUserSessionService userSession;

    public CartController(IMarketplaceService marketplaceService, IUserSessionService userSession)
    {
        this.marketplaceService = marketplaceService;
        this.userSession = userSession;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        if (userSession.IsAdmin)
        {
            TempData["Error"] = "Админ акаунтите не използват количка и поръчки.";
            return RedirectToAction("Index", "Admin");
        }

        if (!userSession.IsAuthenticated || userSession.UserId is null)
        {
            var guestModel = new CartPageViewModel
            {
                RequiresLogin = true,
                InfoMessage = "Разглеждаш като гост. Влез в профила си, за да запазваш продукти в количката."
            };
            return View(guestModel);
        }

        var model = await marketplaceService.GetCartPageAsync(userSession.UserId.Value);
        return View(model);
    }

    [ValidateAntiForgeryToken]
    [HttpPost("quantity")]
    public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
    {
        if (userSession.IsAdmin)
        {
            TempData["Error"] = "Админ акаунтите не използват количка.";
            return RedirectToAction("Index", "Admin");
        }

        if (!userSession.IsAuthenticated || userSession.UserId is null)
        {
            TempData["Error"] = "Моля, влез в профила си, за да управляваш количката.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Index)) });
        }

        await marketplaceService.SetCartItemQuantityAsync(userSession.UserId.Value, productId, quantity);
        return RedirectToAction(nameof(Index));
    }

    [ValidateAntiForgeryToken]
    [HttpPost("remove")]
    public async Task<IActionResult> Remove(int productId)
    {
        if (userSession.IsAdmin)
        {
            TempData["Error"] = "Админ акаунтите не използват количка.";
            return RedirectToAction("Index", "Admin");
        }

        if (!userSession.IsAuthenticated || userSession.UserId is null)
        {
            TempData["Error"] = "Моля, влез в профила си, за да управляваш количката.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Index)) });
        }

        await marketplaceService.RemoveFromCartAsync(userSession.UserId.Value, productId);
        return RedirectToAction(nameof(Index));
    }

    [ValidateAntiForgeryToken]
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(CheckoutInputViewModel input)
    {
        if (userSession.IsAdmin)
        {
            TempData["Error"] = "Админ акаунтите не могат да правят поръчки.";
            return RedirectToAction("Index", "Admin");
        }

        if (!userSession.IsAuthenticated || userSession.UserId is null)
        {
            TempData["Error"] = "Моля, влез в профила си, преди да продължиш към поръчка.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Index)) });
        }

        if (!ModelState.IsValid)
        {
            var invalidModel = await marketplaceService.GetCheckoutPageAsync(userSession.UserId.Value, input);
            return View(invalidModel);
        }

        var result = await marketplaceService.CheckoutAsync(userSession.UserId.Value, input);
        if (!result.Success)
        {
            var model = await marketplaceService.GetCheckoutPageAsync(userSession.UserId.Value, input);
            ModelState.AddModelError(string.Empty, result.Error ?? "Неуспешно завършване на поръчката.");
            return View(model);
        }

        TempData["Success"] = "Поръчката е направена успешно.";
        return RedirectToAction("Index", "Profile", new { tab = "orders" });
    }

    [HttpGet("checkout")]
    public async Task<IActionResult> Checkout()
    {
        if (userSession.IsAdmin)
        {
            TempData["Error"] = "Админ акаунтите не могат да правят поръчки.";
            return RedirectToAction("Index", "Admin");
        }

        if (!userSession.IsAuthenticated || userSession.UserId is null)
        {
            TempData["Error"] = "Моля, влез в профила си, преди да продължиш към поръчка.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Checkout)) });
        }

        var model = await marketplaceService.GetCheckoutPageAsync(userSession.UserId.Value);

        if (model.Items.Count == 0)
        {
            TempData["Error"] = "Количката ти е празна.";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }
}
