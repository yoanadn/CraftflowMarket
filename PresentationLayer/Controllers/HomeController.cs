using Microsoft.AspNetCore.Mvc;
using PresentationLayer.Models;
using PresentationLayer.Infrastructure;
using PresentationLayer.Services;
using System.Diagnostics;

namespace PresentationLayer.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMarketplaceService marketplaceService;
        private readonly IUserSessionService userSession;

        public HomeController(IMarketplaceService marketplaceService, IUserSessionService userSession)
        {
            this.marketplaceService = marketplaceService;
            this.userSession = userSession;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var model = await marketplaceService.GetHomePageAsync(userSession.UserId, search);
            return View(model);
        }

        public IActionResult Privacy()
        {
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Sizes()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
