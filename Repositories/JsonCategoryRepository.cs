using System.Text.Json;
using MarwadiGheeSweetsWeb.Models;

namespace MarwadiGheeSweetsWeb.Repositories;

public class JsonCategoryRepository : ICategoryRepository
{
    private readonly string _jsonPath;
    private List<Category>? _cache;

    private static readonly JsonSerializerOptions _readOpts  = new() { PropertyNameCaseInsensitive = true };
    private static readonly JsonSerializerOptions _writeOpts = new() { WriteIndented = true };

    public JsonCategoryRepository(IWebHostEnvironment env)
    {
        _jsonPath = Path.Combine(env.ContentRootPath, "Data", "categories.json");
    }

    private async Task<List<Category>> LoadAsync()
    {
        if (_cache is not null) return _cache;
        if (!File.Exists(_jsonPath)) return _cache = new();
        var json = await File.ReadAllTextAsync(_jsonPath);
        _cache = JsonSerializer.Deserialize<List<Category>>(json, _readOpts) ?? new();
        return _cache;
    }

    private async Task PersistAsync(List<Category> cats)
    {
        _cache = cats;
        var json = JsonSerializer.Serialize(cats, _writeOpts);
        await File.WriteAllTextAsync(_jsonPath, json);
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
        => (await LoadAsync()).OrderBy(c => c.SortOrder);

    public async Task<Category?> GetBySlugAsync(string slug)
        => (await LoadAsync()).FirstOrDefault(c => c.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));

    public async Task AddAsync(Category category)
    {
        var list = await LoadAsync();
        list.Add(category);
        await PersistAsync(list);
    }

    public async Task UpdateAsync(Category category)
    {
        var list = await LoadAsync();
        var idx  = list.FindIndex(c => c.Id.Equals(category.Id, StringComparison.OrdinalIgnoreCase));
        if (idx >= 0) list[idx] = category;
        await PersistAsync(list);
    }

    public async Task DeleteAsync(string id)
    {
        var list = await LoadAsync();
        list.RemoveAll(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        await PersistAsync(list);
    }
}
