using System.Security.Cryptography;
using System.Text;

namespace MarwadiGheeSweetsWeb.Helpers;

public static class PasswordHelper
{
    // ── BCrypt (current standard) ─────────────────────────────────────────────

    /// <summary>Hashes a password with BCrypt (work factor 12).</summary>
    public static string HashPassword(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

    /// <summary>Verifies a password against a BCrypt hash.</summary>
    public static bool Verify(string password, string bcryptHash)
        => BCrypt.Net.BCrypt.Verify(password, bcryptHash);

    // ── Legacy SHA-256 (migration only) ──────────────────────────────────────

    /// <summary>Returns true if the stored hash looks like a legacy SHA-256 hex string.</summary>
    public static bool IsLegacySha256Hash(string hash)
        => hash.Length == 64 && hash.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'));

    /// <summary>Verifies a password against the legacy SHA-256(salt+password) scheme.</summary>
    public static bool VerifyLegacy(string salt, string password, string sha256Hash)
    {
        var input = Encoding.UTF8.GetBytes(salt + password);
        var hash  = SHA256.HashData(input);
        return Convert.ToHexString(hash).ToLowerInvariant() == sha256Hash;
    }

    // ── Salt generator (kept for legacy OwnerController user-add flow) ────────

    /// <summary>Returns a random 16-byte hex salt (used only during legacy migration).</summary>
    public static string GenerateSalt()
        => Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
}
