using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MarwadiGheeSweetsWeb.Configuration;
using MarwadiGheeSweetsWeb.Services;
using MarwadiGheeSweetsWeb.ViewModels;
using System.Text.Json;

namespace MarwadiGheeSweetsWeb.Controllers;

public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly ShopSettings _settings;

    public ProductsController(
        IProductService productService,
        ICategoryService categoryService,
        IOptions<ShopSettings> settings)
    {
        _productService  = productService;
        _categoryService = categoryService;
        _settings        = settings.Value;
    }

    public async Task<IActionResult> Index(string? category, string? search, string? sort)
    {
        var products   = (await _productService.FilterAndSortAsync(category, search, sort)).ToList();
        var categories = (await _categoryService.GetAllCategoriesAsync()).ToList();

        var selectedCat = categories.FirstOrDefault(c =>
            c.Slug.Equals(category, StringComparison.OrdinalIgnoreCase));

        var vm = new ProductListViewModel
        {
            Products         = products,
            Categories       = categories,
            SelectedCategory = category,
            SearchTerm       = search,
            SortBy           = sort,
            TotalCount       = products.Count,
            MetaTitle        = selectedCat is not null
                ? $"{selectedCat.Name} — {_settings.ShopName} Guntur"
                : $"All Sweets — {_settings.ShopName} Guntur",
            MetaDescription  = selectedCat is not null
                ? $"Buy {selectedCat.Name} online from {_settings.ShopName}, Guntur. Fresh, pure and delicious."
                : $"Shop authentic Indian sweets from {_settings.ShopName}, Guntur. Order online with WhatsApp delivery."
        };

        ViewBag.Settings = _settings;
        return View(vm);
    }

    public async Task<IActionResult> Details(string slug)
    {
        var product = await _productService.GetBySlugAsync(slug);
        if (product is null) return NotFound();

        var related = (await _productService.GetRelatedAsync(product.Id, product.CategorySlug, 4)).ToList();

        var jsonLd = JsonSerializer.Serialize(new
        {
            @context         = "https://schema.org",
            @type            = "Product",
            name             = product.Name,
            description      = product.ShortDescription,
            image            = product.Images.FirstOrDefault(),
            aggregateRating  = new
            {
                @type       = "AggregateRating",
                ratingValue = product.Rating.ToString("F1"),
                reviewCount = product.ReviewCount
            },
            offers = product.Weights.Select(w => new
            {
                @type         = "Offer",
                price         = w.Price.ToString("F0"),
                priceCurrency = "INR",
                availability  = w.IsAvailable
                    ? "https://schema.org/InStock"
                    : "https://schema.org/OutOfStock"
            })
        }, new JsonSerializerOptions { WriteIndented = false });

        var vm = new ProductDetailViewModel
        {
            Product         = product,
            RelatedProducts = related,
            MetaTitle       = $"{product.Name} — {_settings.ShopName} Guntur",
            MetaDescription = product.ShortDescription,
            JsonLd          = jsonLd
        };

        ViewBag.Settings = _settings;
        return View(vm);
    }
}
