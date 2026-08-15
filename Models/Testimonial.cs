namespace MarwadiGheeSweetsWeb.Models;

public class Testimonial
{
    public string Id { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Review { get; set; } = string.Empty;
    public double Rating { get; set; }
    public string? AvatarInitials { get; set; }
    public string? ProductPurchased { get; set; }
    public DateTime ReviewDate { get; set; }
}
