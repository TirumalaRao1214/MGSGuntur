using System.Text.Json;
using MarwadiGheeSweetsWeb.Models;

namespace MarwadiGheeSweetsWeb.Repositories;

public class JsonProductRepository : IProductRepository
{
    private readonly string _jsonPath;
    private List<Product>? _cache;

    private static readonly JsonSerializerOptions _readOpts  = new() { PropertyNameCaseInsensitive = true };
    private static readonly JsonSerializerOptions _writeOpts = new() { WriteIndented = true };

    public JsonProductRepository(IWebHostEnvironment env)
    {
        _jsonPath = Path.Combine(env.ContentRootPath, "Data", "products.json");
    }

    private async Task<List<Product>> LoadAsync()
    {
        if (_cache is not null) return _cache;
        if (!File.Exists(_jsonPath)) return _cache = new();
        var json = await File.ReadAllTextAsync(_jsonPath);
        _cache = JsonSerializer.Deserialize<List<Product>>(json, _readOpts) ?? new();
        return _cache;
    }

    private async Task PersistAsync(List<Product> products)
    {
        _cache = products;
        var json = JsonSerializer.Serialize(products, _writeOpts);
        await File.WriteAllTextAsync(_jsonPath, json);
    }

    public async Task<IEnumerable<Product>> GetAllAsync()       => await LoadAsync();
    public async Task<Product?> GetByIdAsync(string id)         => (await LoadAsync()).FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    public async Task<Product?> GetBySlugAsync(string slug)     => (await LoadAsync()).FirstOrDefault(p => p.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
    public async Task<IEnumerable<Product>> GetByCategoryAsync(string slug) => (await LoadAsync()).Where(p => p.CategorySlug.Equals(slug, StringComparison.OrdinalIgnoreCase));

    public async Task<IEnumerable<Product>> SearchAsync(string searchTerm)
    {
        var all = await LoadAsync();
        return all.Where(p =>
            p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            p.ShortDescription.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            p.Category.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> SlugExistsAsync(string slug, string? excludeId = null)
    {
        var all = await LoadAsync();
        return all.Any(p =>
            p.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase) &&
            (excludeId is null || !p.Id.Equals(excludeId, StringComparison.OrdinalIgnoreCase)));
    }

    public async Task AddAsync(Product product)
    {
        var list = await LoadAsync();
        list.Add(product);
        await PersistAsync(list);
    }

    public async Task UpdateAsync(Product product)
    {
        var list = await LoadAsync();
        var idx  = list.FindIndex(p => p.Id.Equals(product.Id, StringComparison.OrdinalIgnoreCase));
        if (idx >= 0) list[idx] = product;
        await PersistAsync(list);
    }

    public async Task DeleteAsync(string id)
    {
        var list = await LoadAsync();
        list.RemoveAll(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        await PersistAsync(list);
    }
}
