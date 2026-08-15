namespace MarwadiGheeSweetsWeb.Models;

public class OrderSummary
{
    public string OrderId { get; set; } = string.Empty;
    public CustomerDetails Customer { get; set; } = new();
    public List<CartItem> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal DeliveryCharge { get; set; }
    public decimal GrandTotal { get; set; }
    public DateTime OrderedAt { get; set; } = DateTime.Now;
    public string WhatsAppUrl { get; set; } = string.Empty;
}
