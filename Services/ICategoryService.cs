using MarwadiGheeSweetsWeb.Models;

namespace MarwadiGheeSweetsWeb.Services;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllCategoriesAsync();
    Task<Category?> GetCategoryBySlugAsync(string slug);

    // Admin write
    Task AddCategoryAsync(Category category);
    Task UpdateCategoryAsync(Category category);
    Task DeleteCategoryAsync(string id);
}
