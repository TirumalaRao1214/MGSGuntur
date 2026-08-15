using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MarwadiGheeSweetsWeb.Configuration;
using MarwadiGheeSweetsWeb.Helpers;
using MarwadiGheeSweetsWeb.Models;
using MarwadiGheeSweetsWeb.Repositories;
using MarwadiGheeSweetsWeb.Services;
using MarwadiGheeSweetsWeb.ViewModels;

namespace MarwadiGheeSweetsWeb.Controllers;

[Authorize(Roles = "Owner")]
[Route("owner")]
public class OwnerController : Controller
{
    private readonly IUserRepository    _users;
    private readonly IProductService    _products;
    private readonly ICategoryService   _categories;
    private readonly IConfiguration     _config;
    private readonly IWebHostEnvironment _env;

    public OwnerController(
        IUserRepository users,
        IProductService products,
        ICategoryService categories,
        IConfiguration config,
        IWebHostEnvironment env)
    {
        _users      = users;
        _products   = products;
        _categories = categories;
        _config     = config;
        _env        = env;
    }

    // ── Dashboard ──────────────────────────────────────────────────────────

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var all  = (await _products.GetAllForAdminAsync()).ToList();
        var cats = (await _categories.GetAllCategoriesAsync()).ToList();
        var users= (await _users.GetAllAsync()).ToList();
        ViewBag.TotalProducts  = all.Count;
        ViewBag.CategoryCount  = cats.Count;
        ViewBag.AdminUserCount = users.Count(u => u.Role == "Admin");
        return View();
    }

    // ── Shop Settings ──────────────────────────────────────────────────────

    [HttpGet("settings")]
    public IActionResult Settings()
    {
        var s = _config.GetSection("ShopSettings");
        var vm = new OwnerSettingsViewModel
        {
            ShopName              = s["ShopName"]           ?? "",
            TagLine               = s["TagLine"]            ?? "",
            WhatsAppNumber        = s["WhatsAppNumber"]     ?? "",
            Phone                 = s["Phone"]              ?? "",
            Email                 = s["Email"]              ?? "",
            Address               = s["Address"]            ?? "",
            City                  = s["City"]               ?? "",
            State                 = s["State"]              ?? "",
            PinCode               = s["PinCode"]            ?? "",
            StoreHours            = s["StoreHours"]         ?? "",
            GoogleMapsUrl         = s["GoogleMapsUrl"]      ?? "",
            GoogleMapsEmbedUrl    = s["GoogleMapsEmbedUrl"] ?? "",
            InstagramUrl          = s["InstagramUrl"]       ?? "",
            FacebookUrl           = s["FacebookUrl"]        ?? "",
            FreeShippingThreshold = decimal.TryParse(s["FreeShippingThreshold"], out var fst) ? fst : 500,
            DeliveryCharge        = decimal.TryParse(s["DeliveryCharge"],        out var dc)  ? dc  : 50,
            Rating                = double.TryParse(s["Rating"],                 out var r)   ? r   : 4.2,
            ReviewCount           = int.TryParse(s["ReviewCount"],               out var rc)  ? rc  : 0
        };
        return View(vm);
    }

    [HttpPost("settings")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(OwnerSettingsViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        // Read current appsettings.json, update ShopSettings section, write back
        var settingsPath = Path.Combine(_env.ContentRootPath, "appsettings.json");
        var raw  = await System.IO.File.ReadAllTextAsync(settingsPath);
        var root = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(raw)!;

        var shopNode = new
        {
            ShopName              = vm.ShopName,
            TagLine               = vm.TagLine,
            WhatsAppNumber        = vm.WhatsAppNumber,
            Phone                 = vm.Phone,
            Email                 = vm.Email,
            Address               = vm.Address,
            City                  = vm.City,
            State                 = vm.State,
            PinCode               = vm.PinCode,
            StoreHours            = vm.StoreHours,
            GoogleMapsUrl         = vm.GoogleMapsUrl,
            GoogleMapsEmbedUrl    = vm.GoogleMapsEmbedUrl,
            InstagramUrl          = vm.InstagramUrl,
            FacebookUrl           = vm.FacebookUrl,
            FreeShippingThreshold = vm.FreeShippingThreshold,
            DeliveryCharge        = vm.DeliveryCharge,
            Currency              = "₹",
            Rating                = vm.Rating,
            ReviewCount           = vm.ReviewCount
        };

        // Rebuild the whole JSON keeping non-ShopSettings keys intact
        var newRoot = new Dictionary<string, object>();
        foreach (var kv in root)
            newRoot[kv.Key] = kv.Key == "ShopSettings" ? (object)shopNode : (object)kv.Value;

        var opts = new JsonSerializerOptions { WriteIndented = true };
        await System.IO.File.WriteAllTextAsync(settingsPath, JsonSerializer.Serialize(newRoot, opts));

        vm.SuccessMessage = "Settings saved. Restart the app for changes to take full effect.";
        return View(vm);
    }

    // ── User Management ────────────────────────────────────────────────────

    [HttpGet("users")]
    public async Task<IActionResult> Users()
    {
        var vm = new ManageUsersViewModel
        {
            Users = (await _users.GetAllAsync()).ToList()
        };
        return View(vm);
    }

    [HttpPost("users/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddUser(AddUserViewModel form)
    {
        if (!ModelState.IsValid)
        {
            var vm2 = new ManageUsersViewModel
            {
                Users   = (await _users.GetAllAsync()).ToList(),
                AddForm = form,
                ErrorMessage = "Please fix the errors below."
            };
            return View("Users", vm2);
        }

        if (await _users.GetByUsernameAsync(form.Username) is not null)
        {
            var vm2 = new ManageUsersViewModel
            {
                Users   = (await _users.GetAllAsync()).ToList(),
                AddForm = form,
                ErrorMessage = $"Username '{form.Username}' is already taken."
            };
            return View("Users", vm2);
        }

        var salt = PasswordHelper.GenerateSalt();
        var user = new AppUser
        {
            Username     = form.Username.Trim().ToLower(),
            DisplayName  = form.DisplayName.Trim(),
            Role         = form.Role,
            Salt         = salt,
            PasswordHash = PasswordHelper.HashPassword(salt, form.Password),
            IsActive     = true
        };
        await _users.AddAsync(user);
        TempData["Success"] = $"User '{user.DisplayName}' added.";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost("users/delete/{username}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string username)
    {
        // Protect: cannot delete the currently logged-in owner
        if (username.Equals(User.Identity?.Name, StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "You cannot delete your own account.";
            return RedirectToAction(nameof(Users));
        }

        var target = await _users.GetByUsernameAsync(username);
        if (target is null) return NotFound();

        // Protect: last Owner cannot be deleted
        if (target.Role == "Owner")
        {
            var owners = (await _users.GetAllAsync()).Count(u => u.Role == "Owner");
            if (owners <= 1)
            {
                TempData["Error"] = "Cannot delete the only Owner account.";
                return RedirectToAction(nameof(Users));
            }
        }

        await _users.DeleteAsync(username);
        TempData["Success"] = $"User '{username}' removed.";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost("users/toggle/{username}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUser(string username)
    {
        var target = await _users.GetByUsernameAsync(username);
        if (target is null) return NotFound();
        if (username.Equals(User.Identity?.Name, StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "You cannot deactivate your own account.";
            return RedirectToAction(nameof(Users));
        }
        target.IsActive = !target.IsActive;
        var all = (await _users.GetAllAsync()).ToList();
        await _users.SaveAsync(all.Select(u => u.Username == target.Username ? target : u).ToList());
        TempData["Success"] = $"User '{username}' is now {(target.IsActive ? "active" : "inactive")}.";
        return RedirectToAction(nameof(Users));
    }
}
