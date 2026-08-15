using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MarwadiGheeSweetsWeb.Helpers;
using MarwadiGheeSweetsWeb.Repositories;
using MarwadiGheeSweetsWeb.ViewModels;

namespace MarwadiGheeSweetsWeb.Controllers;

public class AuthController : Controller
{
    private readonly IUserRepository _users;

    public AuthController(IUserRepository users) => _users = users;

    [HttpGet("/login")]
    public IActionResult Login(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToRoleHome();

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost("/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _users.GetByUsernameAsync(model.Username);
        if (user is null || !user.IsActive || !PasswordHelper.Verify(user.Salt, model.Password, user.PasswordHash))
        {
            model.Error = "Invalid username or password.";
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name,        user.Username),
            new(ClaimTypes.GivenName,   user.DisplayName),
            new(ClaimTypes.Role,        user.Role),
        };

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToRoleHome();
    }

    [HttpPost("/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    private IActionResult RedirectToRoleHome()
    {
        if (User.IsInRole("Owner")) return RedirectToAction("Index", "Owner");
        if (User.IsInRole("Admin")) return RedirectToAction("Index", "Admin");
        return RedirectToAction("Index", "Home");
    }
}
