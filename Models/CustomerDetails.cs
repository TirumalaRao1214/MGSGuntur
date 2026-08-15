using System.ComponentModel.DataAnnotations;

namespace MarwadiGheeSweetsWeb.Models;

public class CustomerDetails
{
    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone is required")]
    [Phone(ErrorMessage = "Enter a valid phone number")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Order type is required")]
    public string OrderType { get; set; } = "Pickup"; // Pickup | Delivery

    public string? DeliveryAddress { get; set; }

    public string? SpecialInstructions { get; set; }
}
