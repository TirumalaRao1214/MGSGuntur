using System.ComponentModel.DataAnnotations;

namespace MarwadiGheeSweetsWeb.ViewModels;

public class OwnerSettingsViewModel
{
    [Required] public string ShopName          { get; set; } = string.Empty;
    [Required] public string TagLine           { get; set; } = string.Empty;
    [Required] public string WhatsAppNumber    { get; set; } = string.Empty;
    [Required] public string Phone             { get; set; } = string.Empty;
               public string Email             { get; set; } = string.Empty;
    [Required] public string Address           { get; set; } = string.Empty;
    [Required] public string City              { get; set; } = string.Empty;
               public string State             { get; set; } = string.Empty;
               public string PinCode           { get; set; } = string.Empty;
               public string StoreHours        { get; set; } = string.Empty;
               public string GoogleMapsUrl     { get; set; } = string.Empty;
               public string GoogleMapsEmbedUrl{ get; set; } = string.Empty;
               public string InstagramUrl      { get; set; } = string.Empty;
               public string FacebookUrl       { get; set; } = string.Empty;
    [Range(0, 100000)] public decimal FreeShippingThreshold { get; set; }
    [Range(0, 10000)]  public decimal DeliveryCharge        { get; set; }
    [Range(0, 5)]      public double  Rating                { get; set; }
    [Range(0, int.MaxValue)] public int ReviewCount         { get; set; }

    public string? SuccessMessage { get; set; }
}
