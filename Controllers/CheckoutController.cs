using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
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
    private readonly IProductService       _products;
    private readonly ShopSettings          _settings;
    private readonly ILogger<CheckoutController> _logger;

    public CheckoutController(
        IWhatsAppOrderService whatsAppService,
        IProductService productService,
        IOptions<ShopSettings> settings,
        ILogger<CheckoutController> logger)
    {
        _whatsAppService = whatsAppService;
        _products        = productService;
        _settings        = settings.Value;
        _logger          = logger;
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
    public async Task<IActionResult> PlaceOrder(
        [FromForm] CustomerDetails customer,
        [FromForm] string cartJson)
    {
        var normalizedPhone = NormalizePhoneNumber(customer.Phone);
        var sessionVerifiedPhone = HttpContext.Session.GetString("VerifiedPhone");
        if (string.IsNullOrWhiteSpace(normalizedPhone) || !string.Equals(normalizedPhone, sessionVerifiedPhone, StringComparison.Ordinal))
        {
            ModelState.AddModelError(nameof(customer.Phone), "Please verify your phone number before placing the order.");
        }

        // ── Conditional validation: address required for Delivery ─────────────
        if (customer.OrderType == "Delivery" &&
            string.IsNullOrWhiteSpace(customer.DeliveryAddress))
        {
            ModelState.AddModelError(nameof(customer.DeliveryAddress),
                "Delivery address is required for home delivery.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Settings              = _settings;
            ViewBag.FreeShippingThreshold = _settings.FreeShippingThreshold;
            ViewBag.DeliveryCharge        = _settings.DeliveryCharge;
            ViewBag.PhoneVerificationError = ModelState[nameof(customer.Phone)]?.Errors.FirstOrDefault()?.ErrorMessage;
            return View("Index", new CheckoutViewModel
            {
                Customer              = customer,
                FreeShippingThreshold = _settings.FreeShippingThreshold
            });
        }

        // ── Deserialise browser cart (only ProductId, WeightLabel, Quantity trusted) ──
        List<CartItem> browserItems = new();
        try
        {
            if (!string.IsNullOrWhiteSpace(cartJson))
                browserItems = JsonSerializer.Deserialize<List<CartItem>>(cartJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        }
        catch { /* malformed JSON → empty cart, handled below */ }

        if (browserItems.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Your cart is empty.");
            ViewBag.Settings              = _settings;
            ViewBag.FreeShippingThreshold = _settings.FreeShippingThreshold;
            ViewBag.DeliveryCharge        = _settings.DeliveryCharge;
            return View("Index", new CheckoutViewModel
            {
                Customer              = customer,
                FreeShippingThreshold = _settings.FreeShippingThreshold
            });
        }

        // ── SERVER RE-PRICES every line item from the repository ─────────────
        // The browser-supplied Price field is DISCARDED and overwritten here.
        // A malicious user setting price=1 or total=1 is corrected at this point.
        var verifiedItems = new List<CartItem>();
        foreach (var item in browserItems)
        {
            // G2: quantity bounds — already validated by [Range] but re-checked
            //     defensively in case model binding was bypassed.
            if (item.Quantity < 1 || item.Quantity > 50)
            {
                _logger.LogWarning(
                    "Order rejected: invalid quantity {Qty} for product {Id}",
                    item.Quantity, item.ProductId);
                ModelState.AddModelError(string.Empty,
                    $"Invalid quantity for item '{item.ProductName}'. Must be 1–50.");
                ViewBag.Settings              = _settings;
                ViewBag.FreeShippingThreshold = _settings.FreeShippingThreshold;
                ViewBag.DeliveryCharge        = _settings.DeliveryCharge;
                return View("Index", new CheckoutViewModel
                {
                    Customer              = customer,
                    FreeShippingThreshold = _settings.FreeShippingThreshold
                });
            }

            // G1: look up authoritative price from the server-side product store
            var product = await _products.GetByIdAsync(item.ProductId);
            if (product is null || !product.IsAvailable)
            {
                _logger.LogWarning(
                    "Order rejected: product {Id} not found or unavailable.", item.ProductId);
                ModelState.AddModelError(string.Empty,
                    $"'{item.ProductName}' is no longer available. Please remove it from your cart.");
                ViewBag.Settings              = _settings;
                ViewBag.FreeShippingThreshold = _settings.FreeShippingThreshold;
                ViewBag.DeliveryCharge        = _settings.DeliveryCharge;
                return View("Index", new CheckoutViewModel
                {
                    Customer              = customer,
                    FreeShippingThreshold = _settings.FreeShippingThreshold
                });
            }

            var weight = product.Weights.FirstOrDefault(w =>
                w.Label.Equals(item.WeightLabel, StringComparison.OrdinalIgnoreCase)
                && w.IsAvailable);

            if (weight is null)
            {
                _logger.LogWarning(
                    "Order rejected: weight '{Label}' not found for product {Id}.",
                    item.WeightLabel, item.ProductId);
                ModelState.AddModelError(string.Empty,
                    $"Weight option '{item.WeightLabel}' for '{product.Name}' is unavailable.");
                ViewBag.Settings              = _settings;
                ViewBag.FreeShippingThreshold = _settings.FreeShippingThreshold;
                ViewBag.DeliveryCharge        = _settings.DeliveryCharge;
                return View("Index", new CheckoutViewModel
                {
                    Customer              = customer,
                    FreeShippingThreshold = _settings.FreeShippingThreshold
                });
            }

            // Overwrite browser price with the authoritative server price
            verifiedItems.Add(new CartItem
            {
                ProductId   = product.Id,
                ProductSlug = product.Slug,
                ProductName = product.Name,
                ImageUrl    = item.ImageUrl,
                WeightLabel = weight.Label,
                Price       = weight.Price,      // ← SERVER price, never the browser value
                Quantity    = item.Quantity
            });
        }

        // ── All totals calculated server-side from verified items ─────────────
        var subtotal       = verifiedItems.Sum(i => i.LineTotal);
        var deliveryCharge = (customer.OrderType == "Pickup")
            ? 0
            : (subtotal >= _settings.FreeShippingThreshold ? 0 : _settings.DeliveryCharge);
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
            Items               = verifiedItems.Select(i => new WhatsAppOrderItemDto
            {
                Name     = i.ProductName,
                Weight   = i.WeightLabel,
                Quantity = i.Quantity,
                Price    = i.Price
            }).ToList()
        };

        HttpContext.Session.Remove("VerifiedPhone");

        var whatsAppUrl = _whatsAppService.BuildWhatsAppUrl(dto, _settings.WhatsAppNumber);
        return Redirect(whatsAppUrl);
    }

    [HttpPost]
    public async Task<IActionResult> VerifyMsg91AccessToken([FromBody] Msg91AccessTokenRequest request)
    {
        var normalizedPhone = NormalizePhoneNumber(request.Phone);
        if (string.IsNullOrWhiteSpace(request.AccessToken))
        {
            return BadRequest(new { success = false, message = "Access token is required." });
        }

        if (string.IsNullOrWhiteSpace(normalizedPhone))
        {
            return BadRequest(new { success = false, message = "Phone number is required." });
        }

        using var httpClient = new HttpClient();
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://control.msg91.com/api/v5/widget/verifyAccessToken")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["authkey"] = _settings.Msg91AuthKey,
                    ["access-token"] = request.AccessToken
                }),
                Encoding.UTF8,
                "application/json")
        };

        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await httpClient.SendAsync(httpRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, new { success = false, message = responseBody });
        }

        using var document = JsonDocument.Parse(responseBody);
        var root = document.RootElement;
        var isSuccess = root.TryGetProperty("type", out var typeElement)
            && string.Equals(typeElement.GetString(), "success", StringComparison.OrdinalIgnoreCase);

        if (!isSuccess)
        {
            return BadRequest(new { success = false, message = responseBody });
        }

        HttpContext.Session.SetString("VerifiedPhone", normalizedPhone);
        return Ok(new { success = true, verifiedPhone = normalizedPhone });
    }

    private static string NormalizePhoneNumber(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return string.Empty;
        }

        var digits = new string(phone.Where(char.IsDigit).ToArray());
        return digits.Length == 10 ? $"91{digits}" : digits;
    }
}
