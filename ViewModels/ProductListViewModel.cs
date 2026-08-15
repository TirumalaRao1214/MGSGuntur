using MarwadiGheeSweetsWeb.Models;

namespace MarwadiGheeSweetsWeb.ViewModels;

public class ProductListViewModel
{
    public List<Product> Products { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public string? SelectedCategory { get; set; }
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public int TotalCount { get; set; }
    public string MetaTitle { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
}
