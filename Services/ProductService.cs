using MarwadiGheeSweetsWeb.Models;
using MarwadiGheeSweetsWeb.Repositories;

namespace MarwadiGheeSweetsWeb.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepo;

    public ProductService(IProductRepository productRepo) => _productRepo = productRepo;

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
        => (await _productRepo.GetAllAsync()).Where(p => p.IsAvailable).OrderBy(p => p.SortOrder);

    public async Task<IEnumerable<Product>> GetAllForAdminAsync()
        => (await _productRepo.GetAllAsync()).OrderBy(p => p.SortOrder);

    public async Task<IEnumerable<Product>> GetBestSellersAsync(int count = 8)
        => (await _productRepo.GetAllAsync()).Where(p => p.IsAvailable && p.IsBestSeller).OrderBy(p => p.SortOrder).Take(count);

    public async Task<IEnumerable<Product>> GetFeaturedAsync(int count = 6)
        => (await _productRepo.GetAllAsync()).Where(p => p.IsAvailable && p.IsFeatured).OrderBy(p => p.SortOrder).Take(count);

    public async Task<IEnumerable<Product>> GetNewArrivalsAsync(int count = 6)
        => (await _productRepo.GetAllAsync()).Where(p => p.IsAvailable && p.IsNew).OrderBy(p => p.SortOrder).Take(count);

    public async Task<IEnumerable<Product>> GetGiftItemsAsync()
        => (await _productRepo.GetAllAsync()).Where(p => p.IsAvailable && p.IsGiftItem).OrderBy(p => p.SortOrder);

    public async Task<IEnumerable<Product>> GetByCategoryAsync(string categorySlug)
        => (await _productRepo.GetByCategoryAsync(categorySlug)).Where(p => p.IsAvailable).OrderBy(p => p.SortOrder);

    public async Task<IEnumerable<Product>> SearchAsync(string term)
        => (await _productRepo.SearchAsync(term)).Where(p => p.IsAvailable);

    public async Task<IEnumerable<Product>> FilterAndSortAsync(string? category, string? search, string? sortBy)
    {
        IEnumerable<Product> products;
        if (!string.IsNullOrWhiteSpace(search))
            products = await _productRepo.SearchAsync(search);
        else if (!string.IsNullOrWhiteSpace(category) && category != "all")
            products = await _productRepo.GetByCategoryAsync(category);
        else
            products = await _productRepo.GetAllAsync();

        products = products.Where(p => p.IsAvailable);
        products = sortBy switch
        {
            "price-asc"  => products.OrderBy(p => p.BasePrice),
            "price-desc" => products.OrderByDescending(p => p.BasePrice),
            "rating"     => products.OrderByDescending(p => p.Rating),
            "newest"     => products.OrderByDescending(p => p.IsNew).ThenBy(p => p.SortOrder),
            _            => products.OrderBy(p => p.SortOrder)
        };
        return products;
    }

    public async Task<Product?> GetBySlugAsync(string slug) => await _productRepo.GetBySlugAsync(slug);
    public async Task<Product?> GetByIdAsync(string id)     => await _productRepo.GetByIdAsync(id);

    public async Task<IEnumerable<Product>> GetRelatedAsync(string productId, string category, int count = 4)
        => (await _productRepo.GetByCategoryAsync(category))
            .Where(p => p.IsAvailable && p.Id != productId)
            .OrderBy(p => p.SortOrder)
            .Take(count);

    public async Task AddProductAsync(Product product)    => await _productRepo.AddAsync(product);
    public async Task UpdateProductAsync(Product product) => await _productRepo.UpdateAsync(product);
    public async Task DeleteProductAsync(string id)       => await _productRepo.DeleteAsync(id);
    public async Task<bool> SlugExistsAsync(string slug, string? excludeId = null) => await _productRepo.SlugExistsAsync(slug, excludeId);
}
