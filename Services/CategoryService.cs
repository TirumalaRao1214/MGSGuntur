using MarwadiGheeSweetsWeb.Models;
using MarwadiGheeSweetsWeb.Repositories;

namespace MarwadiGheeSweetsWeb.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepo;

    public CategoryService(ICategoryRepository categoryRepo) => _categoryRepo = categoryRepo;

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        => await _categoryRepo.GetAllAsync();

    public async Task<Category?> GetCategoryBySlugAsync(string slug)
        => await _categoryRepo.GetBySlugAsync(slug);

    public async Task AddCategoryAsync(Category category)    => await _categoryRepo.AddAsync(category);
    public async Task UpdateCategoryAsync(Category category) => await _categoryRepo.UpdateAsync(category);
    public async Task DeleteCategoryAsync(string id)         => await _categoryRepo.DeleteAsync(id);
}
