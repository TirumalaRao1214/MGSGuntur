using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using MarwadiGheeSweetsWeb.Models;

namespace MarwadiGheeSweetsWeb.ViewModels;

public class AdminProductViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required] public string Name             { get; set; } = string.Empty;
    [Required] public string Slug             { get; set; } = string.Empty;
    [Required] public string Category         { get; set; } = string.Empty;
    [Required] public string CategorySlug     { get; set; } = string.Empty;
    [Required] public string ShortDescription { get; set; } = string.Empty;
               public string Description      { get; set; } = string.Empty;

    // One image path per line (URLs or /images/products/ paths)
    public string Images { get; set; } = string.Empty;

    // Optional file upload — replaces / prepends to Images if provided
    public IFormFile? ImageUpload { get; set; }

    // Weight rows — parallel lists (up to 4)
    public List<string>   WeightLabels    { get; set; } = new();
    public List<decimal>  WeightPrices    { get; set; } = new();
    public List<decimal?> WeightOriginals { get; set; } = new();
    public List<bool>     WeightAvailable { get; set; } = new();

    [Range(0, 5)] public double Rating      { get; set; } = 4.5;
    [Range(0, int.MaxValue)] public int ReviewCount { get; set; }

    public string IngredientsText      { get; set; } = string.Empty; // comma-separated
    public string AllergensText        { get; set; } = string.Empty; // comma-separated
    public string ShelfLife            { get; set; } = string.Empty;
    public string StorageInstructions  { get; set; } = string.Empty;

    public bool    IsAvailable  { get; set; } = true;
    public bool    IsBestSeller { get; set; }
    public bool    IsNew        { get; set; }
    public bool    IsFeatured   { get; set; }
    public bool    IsGiftItem   { get; set; }
    public string? Badge        { get; set; }
    public int     SortOrder    { get; set; }

    public List<Category> AllCategories { get; set; } = new();
    public bool IsEdit => !string.IsNullOrEmpty(Id);

    public static AdminProductViewModel FromProduct(Product p, List<Category> cats) => new()
    {
        Id                  = p.Id,
        Name                = p.Name,
        Slug                = p.Slug,
        Category            = p.Category,
        CategorySlug        = p.CategorySlug,
        ShortDescription    = p.ShortDescription,
        Description         = p.Description,
        Images              = string.Join("\n", p.Images),
        WeightLabels        = p.Weights.Select(w => w.Label).ToList(),
        WeightPrices        = p.Weights.Select(w => w.Price).ToList(),
        WeightOriginals     = p.Weights.Select(w => w.OriginalPrice).ToList(),
        WeightAvailable     = p.Weights.Select(w => w.IsAvailable).ToList(),
        Rating              = p.Rating,
        ReviewCount         = p.ReviewCount,
        IngredientsText     = string.Join(", ", p.Ingredients),
        AllergensText       = string.Join(", ", p.Allergens),
        ShelfLife           = p.ShelfLife,
        StorageInstructions = p.StorageInstructions,
        IsAvailable         = p.IsAvailable,
        IsBestSeller        = p.IsBestSeller,
        IsNew               = p.IsNew,
        IsFeatured          = p.IsFeatured,
        IsGiftItem          = p.IsGiftItem,
        Badge               = p.Badge,
        SortOrder           = p.SortOrder,
        AllCategories       = cats
    };
}
