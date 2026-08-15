using MarwadiGheeSweetsWeb.Models;

namespace MarwadiGheeSweetsWeb.ViewModels;

public class CheckoutViewModel
{
    public CustomerDetails Customer { get; set; } = new();
    public List<CartItem> CartItems { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal DeliveryCharge { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal FreeShippingThreshold { get; set; }
}
