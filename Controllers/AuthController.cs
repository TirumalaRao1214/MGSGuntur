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
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

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

        // Unknown user — generic error (do not reveal whether user exists)
        if (user is null || !user.IsActive)
        {
            model.Error = "Invalid username or password.";
            return View(model);
        }

        // ── Lockout check ─────────────────────────────────────────────────────
        if (user.LockoutUntil.HasValue && user.LockoutUntil.Value > DateTime.UtcNow)
        {
            var remaining = (int)Math.Ceiling((user.LockoutUntil.Value - DateTime.UtcNow).TotalMinutes);
            model.Error = $"Account locked. Try again in {remaining} minute{(remaining == 1 ? "" : "s")}.";
            return View(model);
        }

        // ── Password verification (BCrypt + legacy SHA-256 migration) ─────────
        bool passwordOk;
        if (PasswordHelper.IsLegacySha256Hash(user.PasswordHash))
        {
            // Legacy SHA-256 hash — verify with old scheme
            passwordOk = PasswordHelper.VerifyLegacy(user.Salt, model.Password, user.PasswordHash);
            if (passwordOk)
            {
                // Silently upgrade to BCrypt on successful login
                user.PasswordHash = PasswordHelper.HashPassword(model.Password);
                user.Salt         = string.Empty; // no longer needed
            }
        }
        else
        {
            passwordOk = PasswordHelper.Verify(model.Password, user.PasswordHash);
        }

        if (!passwordOk)
        {
            // ── Record failed attempt ─────────────────────────────────────────
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= MaxFailedAttempts)
            {
                user.LockoutUntil        = DateTime.UtcNow.Add(LockoutDuration);
                user.FailedLoginAttempts = 0;
                model.Error = $"Too many failed attempts. Account locked for {(int)LockoutDuration.TotalMinutes} minutes.";
            }
            else
            {
                var attemptsLeft = MaxFailedAttempts - user.FailedLoginAttempts;
                model.Error = $"Invalid username or password. {attemptsLeft} attempt{(attemptsLeft == 1 ? "" : "s")} remaining before lockout.";
            }

            var all = (await _users.GetAllAsync()).ToList();
            await _users.SaveAsync(all.Select(u => u.Username == user.Username ? user : u).ToList());
            return View(model);
        }

        // ── Success — reset lockout counters ──────────────────────────────────
        user.FailedLoginAttempts = 0;
        user.LockoutUntil        = null;
        var allUsers = (await _users.GetAllAsync()).ToList();
        await _users.SaveAsync(allUsers.Select(u => u.Username == user.Username ? user : u).ToList());

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name,      user.Username),
            new(ClaimTypes.GivenName, user.DisplayName),
            new(ClaimTypes.Role,      user.Role),
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
