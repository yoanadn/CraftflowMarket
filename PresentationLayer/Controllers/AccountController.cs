using Microsoft.AspNetCore.Mvc;
using PresentationLayer.Infrastructure;
using PresentationLayer.Services;
using PresentationLayer.ViewModels.Auth;

namespace PresentationLayer.Controllers;

[Route("account")]
public class AccountController : Controller
{
    private readonly IAccountService accountService;
    private readonly IUserSessionService userSession;

    public AccountController(IAccountService accountService, IUserSessionService userSession)
    {
        this.accountService = accountService;
        this.userSession = userSession;
    }

    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (userSession.IsAuthenticated)
        {
            return RedirectToAction("Index", "Profile");
        }

        return View(new LoginViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    [ValidateAntiForgeryToken]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await accountService.LoginAsync(model);

        if (!result.Success || result.User is null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Неуспешен вход.");
            return View(model);
        }

        userSession.SignIn(result.User);
        TempData["Success"] = $"Добре дошъл/дошла, {result.User.Username}.";

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Profile");
    }

    [HttpGet("register")]
    public IActionResult Register()
    {
        if (userSession.IsAuthenticated)
        {
            return RedirectToAction("Index", "Profile");
        }

        return View(new RegisterViewModel());
    }

    [ValidateAntiForgeryToken]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await accountService.RegisterAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Неуспешна регистрация.");
            return View(model);
        }

        TempData["Success"] = "Успешна регистрация. Моля, влез в профила си, за да продължиш.";
        return RedirectToAction(nameof(Login));
    }

    [ValidateAntiForgeryToken]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        userSession.SignOut();
        TempData["Success"] = "Излезе успешно от профила си.";
        return RedirectToAction("Index", "Home");
    }
}
