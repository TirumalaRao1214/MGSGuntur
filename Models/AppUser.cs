namespace MarwadiGheeSweetsWeb.Models;

public class AppUser
{
    public string Username     { get; set; } = string.Empty;
    public string DisplayName  { get; set; } = string.Empty;
    public string Role         { get; set; } = string.Empty;  // "Owner" | "Admin"
    public string Salt         { get; set; } = string.Empty;  // legacy SHA-256 salt (kept for migration)
    public string PasswordHash { get; set; } = string.Empty;  // BCrypt hash (or legacy SHA-256 hex during migration)
    public bool   IsActive     { get; set; } = true;

    // ── Brute-force lockout ──────────────────────────────────────────────────
    public int       FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockoutUntil        { get; set; }
}
