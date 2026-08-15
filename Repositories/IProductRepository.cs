using MarwadiGheeSweetsWeb.Models;

namespace MarwadiGheeSweetsWeb.Repositories;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(string id);
    Task<Product?> GetBySlugAsync(string slug);
    Task<IEnumerable<Product>> GetByCategoryAsync(string categorySlug);
    Task<IEnumerable<Product>> SearchAsync(string searchTerm);

    // Write
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(string id);
    Task<bool> SlugExistsAsync(string slug, string? excludeId = null);
}
