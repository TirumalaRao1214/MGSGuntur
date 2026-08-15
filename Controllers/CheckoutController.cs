using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;
using MarwadiGheeSweetsWeb.Configuration;
using MarwadiGheeSweetsWeb.DTOs;
using MarwadiGheeSweetsWeb.Models;
using MarwadiGheeSweetsWeb.Services;
using MarwadiGheeSweetsWeb.ViewModels;

namespace MarwadiGheeSweetsWeb.Controllers;

public class CheckoutController : Controller
{
    private readonly IWhatsAppOrderService _whatsAppService;
    private readonly ShopSettings _settings;

    public CheckoutController(
        IWhatsAppOrderService whatsAppService,
        IOptions<ShopSettings> settings)
    {
        _whatsAppService = whatsAppService;
        _settings        = settings.Value;
    }

    [HttpGet]
    public IActionResult Index()
    {
        ViewBag.Settings              = _settings;
        ViewBag.FreeShippingThreshold = _settings.FreeShippingThreshold;
        ViewBag.DeliveryCharge        = _settings.DeliveryCharge;
        return View(new CheckoutViewModel { FreeShippingThreshold = _settings.FreeShippingThreshold });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult PlaceOrder(
        [FromForm] CustomerDetails customer,
        [FromForm] string cartJson)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Settings              = _settings;
            ViewBag.FreeShippingThreshold = _settings.FreeShippingThreshold;
            ViewBag.DeliveryCharge        = _settings.DeliveryCharge;
            return View("Index", new CheckoutViewModel
            {
                Customer              = customer,
                FreeShippingThreshold = _settings.FreeShippingThreshold
            });
        }

        List<CartItem> cartItems = new();
        try
        {
            if (!string.IsNullOrWhiteSpace(cartJson))
                cartItems = JsonSerializer.Deserialize<List<CartItem>>(cartJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        }
        catch { /* empty cart fallback */ }

        var subtotal       = cartItems.Sum(i => i.LineTotal);
        var deliveryCharge = subtotal >= _settings.FreeShippingThreshold ? 0 : _settings.DeliveryCharge;
        var grandTotal     = subtotal + deliveryCharge;

        var dto = new WhatsAppOrderDto
        {
            CustomerName        = customer.Name,
            Phone               = customer.Phone,
            OrderType           = customer.OrderType,
            DeliveryAddress     = customer.DeliveryAddress,
            SpecialInstructions = customer.SpecialInstructions,
            Subtotal            = subtotal,
            DeliveryCharge      = deliveryCharge,
            GrandTotal          = grandTotal,
            Items               = cartItems.Select(i => new WhatsAppOrderItemDto
            {
                Name     = i.ProductName,
                Weight   = i.WeightLabel,
                Quantity = i.Quantity,
                Price    = i.Price
            }).ToList()
        };

        var whatsAppUrl = _whatsAppService.BuildWhatsAppUrl(dto, _settings.WhatsAppNumber);
        return Redirect(whatsAppUrl);
    }
}
