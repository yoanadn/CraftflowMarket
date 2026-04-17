using Microsoft.AspNetCore.Mvc;
using PresentationLayer.Infrastructure;
using PresentationLayer.Services;

namespace PresentationLayer.Controllers;

[Route("favorites")]
public class FavoritesController : Controller
{
    private readonly IMarketplaceService marketplaceService;
    private readonly IUserSessionService userSession;

    public FavoritesController(IMarketplaceService marketplaceService, IUserSessionService userSession)
    {
        this.marketplaceService = marketplaceService;
        this.userSession = userSession;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        if (userSession.IsAdmin)
        {
            TempData["Error"] = "Админ акаунтите не използват списъци с любими.";
            return RedirectToAction("Index", "Admin");
        }

        if (!userSession.IsAuthenticated || userSession.UserId is null)
        {
            TempData["Error"] = "Моля, влез в профила си, за да видиш любимите продукти.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Index)) });
        }

        var model = await marketplaceService.GetFavoritesPageAsync(userSession.UserId.Value);
        return View(model);
    }
}
