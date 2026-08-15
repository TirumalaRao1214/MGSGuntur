using MarwadiGheeSweetsWeb.Models;

namespace MarwadiGheeSweetsWeb.Services;

public interface IProductService
{
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<IEnumerable<Product>> GetBestSellersAsync(int count = 8);
    Task<IEnumerable<Product>> GetFeaturedAsync(int count = 6);
    Task<IEnumerable<Product>> GetNewArrivalsAsync(int count = 6);
    Task<IEnumerable<Product>> GetGiftItemsAsync();
    Task<IEnumerable<Product>> GetByCategoryAsync(string categorySlug);
    Task<IEnumerable<Product>> SearchAsync(string term);
    Task<IEnumerable<Product>> FilterAndSortAsync(string? category, string? search, string? sortBy);
    Task<Product?> GetBySlugAsync(string slug);
    Task<Product?> GetByIdAsync(string id);
    Task<IEnumerable<Product>> GetRelatedAsync(string productId, string category, int count = 4);

    // Admin write
    Task<IEnumerable<Product>> GetAllForAdminAsync();
    Task AddProductAsync(Product product);
    Task UpdateProductAsync(Product product);
    Task DeleteProductAsync(string id);
    Task<bool> SlugExistsAsync(string slug, string? excludeId = null);
}
