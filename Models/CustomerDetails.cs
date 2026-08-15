using System.ComponentModel.DataAnnotations;

namespace MarwadiGheeSweetsWeb.Models;

public class CustomerDetails
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name must be 100 characters or fewer.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone is required")]
    [Phone(ErrorMessage = "Enter a valid phone number")]
    [StringLength(20, ErrorMessage = "Phone must be 20 characters or fewer.")]
    public string Phone { get; set; } = string.Empty;

    /// <summary>Only "Pickup" or "Delivery" are accepted. Anything else is rejected server-side.</summary>
    [Required(ErrorMessage = "Order type is required")]
    [RegularExpression("^(Pickup|Delivery)$", ErrorMessage = "Order type must be Pickup or Delivery.")]
    public string OrderType { get; set; } = "Pickup";

    /// <summary>Required when OrderType is Delivery — enforced in CheckoutController, not just client-side.</summary>
    [StringLength(300, ErrorMessage = "Delivery address must be 300 characters or fewer.")]
    public string? DeliveryAddress { get; set; }

    [StringLength(500, ErrorMessage = "Special instructions must be 500 characters or fewer.")]
    public string? SpecialInstructions { get; set; }
}
