using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PresentationLayer.Infrastructure;
using PresentationLayer.Services;
using PresentationLayer.ViewModels;

namespace PresentationLayer.Controllers;

[Route("profile")]
public class ProfileController : Controller
{
    private readonly IMarketplaceService marketplaceService;
    private readonly IAccountService accountService;
    private readonly IUserSessionService userSession;

    public ProfileController(IMarketplaceService marketplaceService, IAccountService accountService, IUserSessionService userSession)
    {
        this.marketplaceService = marketplaceService;
        this.accountService = accountService;
        this.userSession = userSession;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? tab)
    {
        if (!userSession.IsAuthenticated || userSession.UserId is null)
        {
            TempData["Error"] = "Моля, влез в профила си, за да отвориш профила си.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Index)) });
        }

        var model = await marketplaceService.GetProfilePageAsync(userSession.UserId.Value, tab);
        return model is null ? NotFound() : View(model);
    }

    [HttpGet("edit")]
    public async Task<IActionResult> Edit()
    {
        if (!userSession.IsAuthenticated || userSession.UserId is null)
        {
            TempData["Error"] = "Моля, влез в профила си, за да редактираш профила си.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Edit)) });
        }

        var model = await accountService.GetProfileEditAsync(userSession.UserId.Value);
        return model is null ? NotFound() : View(model);
    }

    [ValidateAntiForgeryToken]
    [HttpPost("edit")]
    public async Task<IActionResult> Edit(ProfileEditViewModel model, IFormFile? imageFile)
    {
        if (!userSession.IsAuthenticated || userSession.UserId is null)
        {
            TempData["Error"] = "Моля, влез в профила си, за да редактираш профила си.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Edit)) });
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await accountService.UpdateProfileAsync(userSession.UserId.Value, model, imageFile);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Неуспешно обновяване на профила.");
            return View(model);
        }

        TempData["Success"] = "Профилът е обновен успешно.";
        return RedirectToAction(nameof(Index));
    }

    [ValidateAntiForgeryToken]
    [HttpPost("products")]
    public async Task<IActionResult> AddProduct([Bind(Prefix = "NewProduct")] ProfileCreateProductViewModel model, List<IFormFile>? imageFiles)
    {
        if (!userSession.IsAuthenticated || userSession.UserId is null)
        {
            TempData["Error"] = "Моля, влез в профила си, за да добавяш продукти.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Index), new { tab = "products" }) });
        }

        if (userSession.IsAdmin)
        {
            TempData["Error"] = "Админ акаунтите не могат да публикуват продукти за продажба.";
            return RedirectToAction("Index", "Admin");
        }

        if (!ModelState.IsValid)
        {
            var invalidModel = await marketplaceService.GetProfilePageAsync(userSession.UserId.Value, "products");
            if (invalidModel is null)
            {
                return NotFound();
            }

            invalidModel.NewProduct = model;
            return View("Index", invalidModel);
        }

        var result = await marketplaceService.AddProductForUserAsync(userSession.UserId.Value, model, imageFiles);

        if (!result.Success)
        {
            var failedModel = await marketplaceService.GetProfilePageAsync(userSession.UserId.Value, "products");
            if (failedModel is null)
            {
                return NotFound();
            }

            ModelState.AddModelError(string.Empty, result.Error ?? "Неуспешно добавяне на продукт.");
            failedModel.NewProduct = model;
            return View("Index", failedModel);
        }

        TempData["Success"] = "Продуктът е добавен успешно.";
        return RedirectToAction(nameof(Index), new { tab = "products" });
    }

    [HttpGet("products/{productId:int}/edit")]
    public async Task<IActionResult> EditProduct(int productId)
    {
        if (!userSession.IsAuthenticated || userSession.UserId is null)
        {
            TempData["Error"] = "Моля, влез в профила си, за да редактираш продукта си.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(EditProduct), new { productId }) });
        }

        if (userSession.IsAdmin)
        {
            TempData["Error"] = "Админ акаунтите не могат да редактират продукти на продавачи оттук.";
            return RedirectToAction("Index", "Admin");
        }

        var model = await marketplaceService.GetProfileEditProductAsync(userSession.UserId.Value, productId);
        return model is null ? NotFound() : View(model);
    }

    [ValidateAntiForgeryToken]
    [HttpPost("products/{productId:int}/edit")]
    public async Task<IActionResult> EditProduct(int productId, ProfileEditProductViewModel model, List<IFormFile>? imageFiles)
    {
        if (!userSession.IsAuthenticated || userSession.UserId is null)
        {
            TempData["Error"] = "Моля, влез в профила си, за да редактираш продукта си.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(EditProduct), new { productId }) });
        }

        if (userSession.IsAdmin)
        {
            TempData["Error"] = "Админ акаунтите не могат да редактират продукти на продавачи оттук.";
            return RedirectToAction("Index", "Admin");
        }

        model.Id = productId;

        if (!ModelState.IsValid)
        {
            var invalidModel = await marketplaceService.GetProfileEditProductAsync(userSession.UserId.Value, productId);
            if (invalidModel is null)
            {
                return NotFound();
            }

            invalidModel.Name = model.Name;
            invalidModel.Category = model.Category;
            invalidModel.Price = model.Price;
            invalidModel.Description = model.Description;
            invalidModel.GenderTag = model.GenderTag;
            invalidModel.ColorTag = model.ColorTag;
            invalidModel.SizeTags = model.SizeTags;
            invalidModel.RemoveImageIds = model.RemoveImageIds;
            return View(invalidModel);
        }

        var result = await marketplaceService.UpdateProductByOwnerAsync(userSession.UserId.Value, model, imageFiles);
        if (!result.Success)
        {
            var failedModel = await marketplaceService.GetProfileEditProductAsync(userSession.UserId.Value, productId);
            if (failedModel is null)
            {
                return NotFound();
            }

            failedModel.Name = model.Name;
            failedModel.Category = model.Category;
            failedModel.Price = model.Price;
            failedModel.Description = model.Description;
            failedModel.GenderTag = model.GenderTag;
            failedModel.ColorTag = model.ColorTag;
            failedModel.SizeTags = model.SizeTags;
            failedModel.RemoveImageIds = model.RemoveImageIds;
            ModelState.AddModelError(string.Empty, result.Error ?? "Неуспешно обновяване на продукта.");
            return View(failedModel);
        }

        TempData["Success"] = "Продуктът е обновен успешно.";
        return RedirectToAction(nameof(Index), new { tab = "products" });
    }

    [ValidateAntiForgeryToken]
    [HttpPost("products/{productId:int}/delete")]
    public async Task<IActionResult> DeleteProduct(int productId)
    {
        if (!userSession.IsAuthenticated || userSession.UserId is null)
        {
            TempData["Error"] = "Моля, влез в профила си, за да управляваш продуктите си.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Index), new { tab = "products" }) });
        }

        if (userSession.IsAdmin)
        {
            TempData["Error"] = "Админ акаунтите не могат да изтриват продукти на продавачи оттук.";
            return RedirectToAction("Index", "Admin");
        }

        var result = await marketplaceService.DeleteProductForUserAsync(userSession.UserId.Value, productId);
        TempData[result.Success ? "Success" : "Error"] = result.Success
            ? "Продуктът е изтрит успешно."
            : result.Error ?? "Неуспешно изтриване на продукта.";

        return RedirectToAction(nameof(Index), new { tab = "products" });
    }

    [ValidateAntiForgeryToken]
    [HttpPost("delete")]
    public async Task<IActionResult> DeleteProfile()
    {
        if (!userSession.IsAuthenticated || userSession.UserId is null)
        {
            TempData["Error"] = "Моля, влез в профила си, за да го изтриеш.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Index)) });
        }

        if (userSession.IsAdmin)
        {
            TempData["Error"] = "Админ акаунтът не може да се изтрие от тази страница.";
            return RedirectToAction("Index", "Admin");
        }

        var userId = userSession.UserId.Value;
        await marketplaceService.DeleteUserAsync(userId);
        userSession.SignOut();

        TempData["Success"] = "Профилът ти беше изтрит успешно.";
        return RedirectToAction("Index", "Home");
    }
}
