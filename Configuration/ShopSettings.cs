namespace MarwadiGheeSweetsWeb.Configuration;

public class ShopSettings
{
    public const string SectionName = "ShopSettings";

    public string ShopName { get; set; } = string.Empty;
    public string TagLine { get; set; } = string.Empty;
    public string WhatsAppNumber { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PinCode { get; set; } = string.Empty;
    public string StoreHours { get; set; } = string.Empty;
    public string GoogleMapsUrl { get; set; } = string.Empty;
    public string GoogleMapsEmbedUrl { get; set; } = string.Empty;
    public string InstagramUrl { get; set; } = string.Empty;
    public string FacebookUrl { get; set; } = string.Empty;
    public decimal FreeShippingThreshold { get; set; }
    public decimal DeliveryCharge { get; set; }
    public string Currency { get; set; } = "₹";
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
}
