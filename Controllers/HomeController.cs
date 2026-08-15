using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MarwadiGheeSweetsWeb.Configuration;
using MarwadiGheeSweetsWeb.Services;
using MarwadiGheeSweetsWeb.ViewModels;

namespace MarwadiGheeSweetsWeb.Controllers;

public class HomeController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly ShopSettings _settings;

    public HomeController(
        IProductService productService,
        ICategoryService categoryService,
        IOptions<ShopSettings> settings)
    {
        _productService = productService;
        _categoryService = categoryService;
        _settings = settings.Value;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new HomeViewModel
        {
            BestSellers      = (await _productService.GetBestSellersAsync(8)).ToList(),
            FeaturedProducts = (await _productService.GetFeaturedAsync(6)).ToList(),
            NewArrivals      = (await _productService.GetNewArrivalsAsync(4)).ToList(),
            GiftHampers      = (await _productService.GetGiftItemsAsync()).ToList(),
            Categories       = (await _categoryService.GetAllCategoriesAsync()).ToList(),
            Testimonials     = GetTestimonials(),
            MetaTitle        = $"{_settings.ShopName} — Best Sweets in Guntur, Andhra Pradesh",
            MetaDescription  = $"Buy authentic ghee sweets, kaju sweets and Indian mithai online from {_settings.ShopName}, Guntur. " +
                               "Rated 4.2★ by 1450+ customers. Order on WhatsApp for same-day delivery."
        };

        ViewBag.Settings = _settings;
        return View(vm);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();

    private static List<MarwadiGheeSweetsWeb.Models.Testimonial> GetTestimonials() => new()
    {
        new() { Id = "t1", CustomerName = "Priya Raju", Location = "Guntur", Rating = 5,
            Review = "The Ghee Mysore Pak here is absolutely unmatched! My family has been buying from Marwadi Ghee Sweets for 15 years. Pure ghee, perfect texture — no one does it better in Guntur.",
            ProductPurchased = "Ghee Mysore Pak", AvatarInitials = "PR",
            ReviewDate = new DateTime(2024, 10, 12) },

        new() { Id = "t2", CustomerName = "Srinivas Rao", Location = "Vijayawada", Rating = 5,
            Review = "Ordered the Festival Sweet Box for Diwali — everyone at the office loved it! The Kaala Jamun is something else. Soft, rich, and perfectly sweetened. Will order again for Christmas!",
            ProductPurchased = "Festival Sweet Box", AvatarInitials = "SR",
            ReviewDate = new DateTime(2024, 11, 3) },

        new() { Id = "t3", CustomerName = "Meena Kumari", Location = "Guntur", Rating = 4,
            Review = "The Kaju Katli melts in your mouth. So fresh and so generous with the cashews. I drove 20km just to pick these up — totally worth it!",
            ProductPurchased = "Kaju Katli", AvatarInitials = "MK",
            ReviewDate = new DateTime(2024, 9, 28) },

        new() { Id = "t4", CustomerName = "Venkat Reddy", Location = "Hyderabad", Rating = 5,
            Review = "Been ordering their sweets for corporate gifting for 3 years. Always delivered fresh, beautifully packed, and clients are always impressed. WhatsApp ordering is super convenient!",
            ProductPurchased = "Corporate Gift Box", AvatarInitials = "VR",
            ReviewDate = new DateTime(2024, 12, 1) },

        new() { Id = "t5", CustomerName = "Lakshmi Devi", Location = "Guntur", Rating = 5,
            Review = "The Pootharekulu is absolutely divine — thin as paper and melt-in-mouth. I've never tasted anything like it outside of Atreyapuram. They've nailed it here!",
            ProductPurchased = "Pootharekulu", AvatarInitials = "LD",
            ReviewDate = new DateTime(2024, 11, 20) },

        new() { Id = "t6", CustomerName = "Arun Kumar", Location = "Guntur", Rating = 4,
            Review = "Motichur laddus are perfect for temple offerings. Fresh, fragrant, and just the right sweetness. Also love their Telugu Mixture — a staple in my house now.",
            ProductPurchased = "Motichur Laddu", AvatarInitials = "AK",
            ReviewDate = new DateTime(2024, 10, 5) }
    };
}
