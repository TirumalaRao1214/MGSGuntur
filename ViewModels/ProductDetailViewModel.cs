using MarwadiGheeSweetsWeb.Models;

namespace MarwadiGheeSweetsWeb.ViewModels;

public class ProductDetailViewModel
{
    public Product Product { get; set; } = new();
    public List<Product> RelatedProducts { get; set; } = new();
    public string MetaTitle { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string JsonLd { get; set; } = string.Empty;
}
