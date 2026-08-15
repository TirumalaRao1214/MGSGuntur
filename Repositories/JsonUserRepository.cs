using System.Text.Json;
using MarwadiGheeSweetsWeb.Models;

namespace MarwadiGheeSweetsWeb.Repositories;

public class JsonUserRepository : IUserRepository
{
    private readonly string _jsonPath;
    private List<AppUser>? _cache;

    private static readonly JsonSerializerOptions _readOpts = new() { PropertyNameCaseInsensitive = true };
    private static readonly JsonSerializerOptions _writeOpts = new() { WriteIndented = true };

    public JsonUserRepository(IWebHostEnvironment env)
    {
        _jsonPath = Path.Combine(env.ContentRootPath, "Data", "users.json");
    }

    private async Task<List<AppUser>> LoadAsync()
    {
        if (_cache is not null) return _cache;
        if (!File.Exists(_jsonPath)) return _cache = new();
        var json = await File.ReadAllTextAsync(_jsonPath);
        _cache = JsonSerializer.Deserialize<List<AppUser>>(json, _readOpts) ?? new();
        return _cache;
    }

    public async Task<IEnumerable<AppUser>> GetAllAsync() => await LoadAsync();

    public async Task<AppUser?> GetByUsernameAsync(string username)
        => (await LoadAsync()).FirstOrDefault(u =>
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

    public async Task AddAsync(AppUser user)
    {
        var list = await LoadAsync();
        list.Add(user);
        await SaveAsync(list);
    }

    public async Task DeleteAsync(string username)
    {
        var list = await LoadAsync();
        list.RemoveAll(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        await SaveAsync(list);
    }

    public async Task SaveAsync(List<AppUser> users)
    {
        _cache = users;
        var json = JsonSerializer.Serialize(users, _writeOpts);
        await File.WriteAllTextAsync(_jsonPath, json);
    }
}
