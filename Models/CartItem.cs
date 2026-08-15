using System.ComponentModel.DataAnnotations;

namespace MarwadiGheeSweetsWeb.Models;

public class CartItem
{
    public string ProductId   { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ImageUrl    { get; set; } = string.Empty;
    public string WeightLabel { get; set; } = string.Empty;

    /// <summary>
    /// Populated from browser JSON but NEVER used for price calculation.
    /// The server overwrites this from the product repository before any
    /// totals are computed (see CheckoutController.PlaceOrder).
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>Server-enforced: 1–50 units per line item.</summary>
    [Range(1, 50, ErrorMessage = "Quantity must be between 1 and 50.")]
    public int Quantity { get; set; }

    public decimal LineTotal => Price * Quantity;
}
