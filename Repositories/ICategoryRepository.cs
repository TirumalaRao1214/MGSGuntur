using MarwadiGheeSweetsWeb.Models;

namespace MarwadiGheeSweetsWeb.Repositories;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetBySlugAsync(string slug);

    // Write
    Task AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(string id);
}
