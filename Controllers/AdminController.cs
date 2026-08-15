using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarwadiGheeSweetsWeb.Models;
using MarwadiGheeSweetsWeb.Services;
using MarwadiGheeSweetsWeb.ViewModels;
using Microsoft.AspNetCore.Hosting;

namespace MarwadiGheeSweetsWeb.Controllers;

[Authorize(Roles = "Admin,Owner")]
[Route("admin")]
public class AdminController : Controller
{
    private readonly IProductService  _products;
    private readonly ICategoryService _categories;
    private readonly IWebHostEnvironment _env;

    public AdminController(IProductService products, ICategoryService categories, IWebHostEnvironment env)
    {
        _products   = products;
        _categories = categories;
        _env        = env;
    }

    // ── Dashboard ──────────────────────────────────────────────────────────

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var all  = (await _products.GetAllForAdminAsync()).ToList();
        var cats = (await _categories.GetAllCategoriesAsync()).ToList();
        ViewBag.TotalProducts    = all.Count;
        ViewBag.AvailableCount   = all.Count(p => p.IsAvailable);
        ViewBag.BestSellerCount  = all.Count(p => p.IsBestSeller);
        ViewBag.CategoryCount    = cats.Count;
        ViewBag.RecentProducts   = all.TakeLast(5).Reverse().ToList();
        return View();
    }

    // ── Products list ──────────────────────────────────────────────────────

    [HttpGet("products")]
    public async Task<IActionResult> Products()
    {
        var all = (await _products.GetAllForAdminAsync()).ToList();
        return View(all);
    }

    // ── Create ─────────────────────────────────────────────────────────────

    [HttpGet("products/create")]
    public async Task<IActionResult> CreateProduct()
    {
        var cats = (await _categories.GetAllCategoriesAsync()).ToList();
        var vm   = new AdminProductViewModel
        {
            AllCategories  = cats,
            WeightLabels   = new() { "250g", "500g", "1kg" },
            WeightPrices   = new() { 0, 0, 0 },
            WeightOriginals= new() { null, null, null },
            WeightAvailable= new() { true, true, true }
        };
        return View("ProductForm", vm);
    }

    [HttpPost("products/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProduct(AdminProductViewModel vm)
    {
        await EnsureCategoriesAsync(vm);
        if (!ModelState.IsValid) return View("ProductForm", vm);

        if (await _products.SlugExistsAsync(vm.Slug))
        {
            ModelState.AddModelError(nameof(vm.Slug), "This slug is already in use.");
            return View("ProductForm", vm);
        }

        var uploadedPath = await SaveUploadedImageAsync(vm);
        if (uploadedPath is not null)
            vm.Images = uploadedPath + "\n" + vm.Images;

        var product = BuildProduct(vm, "prod-" + Guid.NewGuid().ToString("N")[..8]);
        await _products.AddProductAsync(product);
        TempData["Success"] = $"'{product.Name}' created successfully.";
        return RedirectToAction(nameof(Products));
    }

    // ── Edit ───────────────────────────────────────────────────────────────

    [HttpGet("products/edit/{id}")]
    public async Task<IActionResult> EditProduct(string id)
    {
        var product = await _products.GetByIdAsync(id);
        if (product is null) return NotFound();
        var cats = (await _categories.GetAllCategoriesAsync()).ToList();
        return View("ProductForm", AdminProductViewModel.FromProduct(product, cats));
    }

    [HttpPost("products/edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProduct(string id, AdminProductViewModel vm)
    {
        await EnsureCategoriesAsync(vm);
        if (!ModelState.IsValid) return View("ProductForm", vm);

        if (await _products.SlugExistsAsync(vm.Slug, id))
        {
            ModelState.AddModelError(nameof(vm.Slug), "This slug is already in use by another product.");
            return View("ProductForm", vm);
        }

        // Capture existing images before overwriting, so we can delete old files.
        var existing = await _products.GetByIdAsync(id);
        var oldImages = existing?.Images ?? new List<string>();

        var uploadedPath = await SaveUploadedImageAsync(vm);
        if (uploadedPath is not null)
        {
            // Delete old local image files that are being replaced.
            DeleteOldImages(oldImages);
            vm.Images = uploadedPath;
        }

        var product = BuildProduct(vm, id);
        await _products.UpdateProductAsync(product);
        TempData["Success"] = $"'{product.Name}' updated successfully.";
        return RedirectToAction(nameof(Products));
    }

    // ── Toggle availability ────────────────────────────────────────────────

    [HttpPost("products/toggle/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAvailability(string id)
    {
        var product = await _products.GetByIdAsync(id);
        if (product is null) return NotFound();
        product.IsAvailable = !product.IsAvailable;
        await _products.UpdateProductAsync(product);
        TempData["Success"] = $"'{product.Name}' is now {(product.IsAvailable ? "available" : "unavailable")}.";
        return RedirectToAction(nameof(Products));
    }

    // ── Delete ─────────────────────────────────────────────────────────────

    [HttpPost("products/delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        var product = await _products.GetByIdAsync(id);
        if (product is null) return NotFound();
        await _products.DeleteProductAsync(id);
        TempData["Success"] = $"'{product.Name}' deleted.";
        return RedirectToAction(nameof(Products));
    }

    // ── Categories ─────────────────────────────────────────────────────────

    [HttpGet("categories")]
    public async Task<IActionResult> Categories()
    {
        var cats = (await _categories.GetAllCategoriesAsync()).ToList();
        return View(cats);
    }

    [HttpPost("categories/save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCategory(Category cat)
    {
        if (string.IsNullOrWhiteSpace(cat.Name))
        {
            TempData["Error"] = "Category name is required.";
            return RedirectToAction(nameof(Categories));
        }

        if (string.IsNullOrEmpty(cat.Id))
        {
            cat.Id = "cat-" + Guid.NewGuid().ToString("N")[..8];
            if (string.IsNullOrWhiteSpace(cat.Slug))
                cat.Slug = cat.Name.ToLower().Replace(" ", "-");
            await _categories.AddCategoryAsync(cat);
            TempData["Success"] = $"Category '{cat.Name}' added.";
        }
        else
        {
            await _categories.UpdateCategoryAsync(cat);
            TempData["Success"] = $"Category '{cat.Name}' updated.";
        }
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost("categories/delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(string id)
    {
        await _categories.DeleteCategoryAsync(id);
        TempData["Success"] = "Category deleted.";
        return RedirectToAction(nameof(Categories));
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private async Task EnsureCategoriesAsync(AdminProductViewModel vm)
        => vm.AllCategories = (await _categories.GetAllCategoriesAsync()).ToList();

    /// <summary>
    /// Saves an uploaded image file to wwwroot/images/products/ and returns the
    /// web-relative path, or null if no file was uploaded.
    /// </summary>
    private async Task<string?> SaveUploadedImageAsync(AdminProductViewModel vm)
    {
        var file = vm.ImageUpload;
        if (file is null || file.Length == 0) return null;

        // Sanitise filename: use slug + original extension, lower-cased
        var ext      = Path.GetExtension(file.FileName).ToLowerInvariant();
        var safeName = string.IsNullOrWhiteSpace(vm.Slug)
            ? Guid.NewGuid().ToString("N")[..12] + ext
            : vm.Slug.Trim().ToLower() + ext;

        var folder   = Path.Combine(_env.WebRootPath, "images", "products");
        Directory.CreateDirectory(folder);                       // ensure exists
        var fullPath = Path.Combine(folder, safeName);

        using var stream = System.IO.File.Create(fullPath);
        await file.CopyToAsync(stream);

        return "/images/products/" + safeName;
    }

    /// <summary>
    /// Deletes image files that live under wwwroot (i.e. paths starting with "/images/").
    /// External URLs (http/https) are left untouched.
    /// </summary>
    private void DeleteOldImages(IEnumerable<string> imagePaths)
    {
        foreach (var path in imagePaths)
        {
            if (string.IsNullOrWhiteSpace(path)) continue;
            if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase)) continue;

            var fullPath = Path.Combine(_env.WebRootPath, path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }
    }

    private static Product BuildProduct(AdminProductViewModel vm, string id)
    {
        var weights = new List<ProductWeight>();
        for (int i = 0; i < vm.WeightLabels.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(vm.WeightLabels[i])) continue;
            weights.Add(new ProductWeight
            {
                Label         = vm.WeightLabels[i],
                Price         = i < vm.WeightPrices.Count    ? vm.WeightPrices[i]    : 0,
                OriginalPrice = i < vm.WeightOriginals.Count ? vm.WeightOriginals[i] : null,
                IsAvailable   = i < vm.WeightAvailable.Count ? vm.WeightAvailable[i] : true
            });
        }

        return new Product
        {
            Id                  = id,
            Name                = vm.Name.Trim(),
            Slug                = vm.Slug.Trim().ToLower(),
            Category            = vm.Category.Trim(),
            CategorySlug        = vm.CategorySlug.Trim().ToLower(),
            ShortDescription    = vm.ShortDescription.Trim(),
            Description         = vm.Description.Trim(),
            Images              = vm.Images.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
            Weights             = weights,
            Rating              = vm.Rating,
            ReviewCount         = vm.ReviewCount,
            Ingredients         = vm.IngredientsText.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
            Allergens           = vm.AllergensText.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
            ShelfLife           = vm.ShelfLife,
            StorageInstructions = vm.StorageInstructions,
            IsAvailable         = vm.IsAvailable,
            IsBestSeller        = vm.IsBestSeller,
            IsNew               = vm.IsNew,
            IsFeatured          = vm.IsFeatured,
            IsGiftItem          = vm.IsGiftItem,
            Badge               = string.IsNullOrWhiteSpace(vm.Badge) ? null : vm.Badge.Trim(),
            SortOrder           = vm.SortOrder
        };
    }
}
