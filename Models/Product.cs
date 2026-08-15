namespace MarwadiGheeSweetsWeb.Models;

public class Product
{
    public string Id { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Images { get; set; } = new();
    public List<ProductWeight> Weights { get; set; } = new();
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public List<string> Ingredients { get; set; } = new();
    public List<string> Allergens { get; set; } = new();
    public string ShelfLife { get; set; } = string.Empty;
    public string StorageInstructions { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public bool IsBestSeller { get; set; }
    public bool IsNew { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsGiftItem { get; set; }
    public string? Badge { get; set; }
    public int SortOrder { get; set; }

    public decimal BasePrice => Weights.FirstOrDefault()?.Price ?? 0;
    public decimal? OriginalBasePrice => Weights.FirstOrDefault()?.OriginalPrice;
    public string PrimaryImage => Images.FirstOrDefault() ?? "/images/placeholder.jpg";
}
