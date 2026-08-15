using System.Text;
using MarwadiGheeSweetsWeb.DTOs;

namespace MarwadiGheeSweetsWeb.Services;

public class WhatsAppOrderService : IWhatsAppOrderService
{
    public string FormatOrderMessage(WhatsAppOrderDto order)
    {
        var sb = new StringBuilder();
        sb.AppendLine("🍬 *New Order — Marwadi Ghee Sweets*");
        sb.AppendLine("─────────────────────────");
        sb.AppendLine($"👤 *Name:* {order.CustomerName}");
        sb.AppendLine($"📞 *Phone:* {order.Phone}");
        sb.AppendLine($"📦 *Order Type:* {order.OrderType}");

        if (order.OrderType == "Delivery" && !string.IsNullOrWhiteSpace(order.DeliveryAddress))
            sb.AppendLine($"🏠 *Delivery Address:* {order.DeliveryAddress}");

        sb.AppendLine("─────────────────────────");
        sb.AppendLine("🛒 *Order Items:*");

        foreach (var item in order.Items)
            sb.AppendLine($"  • {item.Name} ({item.Weight}) × {item.Quantity} = ₹{item.Total:F0}");

        sb.AppendLine("─────────────────────────");
        sb.AppendLine($"🧾 *Subtotal:* ₹{order.Subtotal:F0}");

        if (order.DeliveryCharge > 0)
            sb.AppendLine($"🚚 *Delivery:* ₹{order.DeliveryCharge:F0}");
        else
            sb.AppendLine("🚚 *Delivery:* FREE ✅");

        sb.AppendLine($"💰 *Grand Total: ₹{order.GrandTotal:F0}*");

        if (!string.IsNullOrWhiteSpace(order.SpecialInstructions))
        {
            sb.AppendLine("─────────────────────────");
            sb.AppendLine($"📝 *Special Note:* {order.SpecialInstructions}");
        }

        sb.AppendLine("─────────────────────────");
        sb.AppendLine("Thank you for ordering from Marwadi Ghee Sweets, Guntur! 🙏");

        return sb.ToString();
    }

    public string BuildWhatsAppUrl(WhatsAppOrderDto order, string whatsAppNumber)
    {
        var message = FormatOrderMessage(order);
        var encoded = Uri.EscapeDataString(message);
        return $"https://wa.me/{whatsAppNumber}?text={encoded}";
    }
}
