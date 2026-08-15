namespace MarwadiGheeSweetsWeb.Models;

public class AppUser
{
    public string Username     { get; set; } = string.Empty;
    public string DisplayName  { get; set; } = string.Empty;
    public string Role         { get; set; } = string.Empty;  // "Owner" | "Admin"
    public string Salt         { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;  // SHA-256(salt + password)
    public bool   IsActive     { get; set; } = true;
}
