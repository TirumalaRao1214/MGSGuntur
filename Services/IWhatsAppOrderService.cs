using MarwadiGheeSweetsWeb.DTOs;

namespace MarwadiGheeSweetsWeb.Services;

public interface IWhatsAppOrderService
{
    string FormatOrderMessage(WhatsAppOrderDto order);
    string BuildWhatsAppUrl(WhatsAppOrderDto order, string whatsAppNumber);
}
