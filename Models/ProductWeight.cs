namespace MarwadiGheeSweetsWeb.Models;

public class ProductWeight
{
    public string Label { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public bool IsAvailable { get; set; } = true;
}
