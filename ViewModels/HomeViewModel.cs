using MarwadiGheeSweetsWeb.Models;

namespace MarwadiGheeSweetsWeb.ViewModels;

public class HomeViewModel
{
    public List<Product> BestSellers { get; set; } = new();
    public List<Product> FeaturedProducts { get; set; } = new();
    public List<Product> NewArrivals { get; set; } = new();
    public List<Product> GiftHampers { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<Testimonial> Testimonials { get; set; } = new();
    public string MetaTitle { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
}
