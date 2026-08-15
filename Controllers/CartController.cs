using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MarwadiGheeSweetsWeb.Configuration;
using MarwadiGheeSweetsWeb.ViewModels;

namespace MarwadiGheeSweetsWeb.Controllers;

public class CartController : Controller
{
    private readonly ShopSettings _settings;

    public CartController(IOptions<ShopSettings> settings)
    {
        _settings = settings.Value;
    }

    public IActionResult Index()
    {
        ViewBag.Settings               = _settings;
        ViewBag.FreeShippingThreshold  = _settings.FreeShippingThreshold;
        ViewBag.DeliveryCharge         = _settings.DeliveryCharge;
        return View();
    }
}
