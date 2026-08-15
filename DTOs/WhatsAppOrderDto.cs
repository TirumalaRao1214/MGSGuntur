namespace MarwadiGheeSweetsWeb.DTOs;

public class WhatsAppOrderDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string OrderType { get; set; } = "Pickup";
    public string? DeliveryAddress { get; set; }
    public string? SpecialInstructions { get; set; }
    public List<WhatsAppOrderItemDto> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal DeliveryCharge { get; set; }
    public decimal GrandTotal { get; set; }
}

public class WhatsAppOrderItemDto
{
    public string Name { get; set; } = string.Empty;
    public string Weight { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total => Price * Quantity;
}
