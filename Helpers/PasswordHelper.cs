using System.Security.Cryptography;
using System.Text;

namespace MarwadiGheeSweetsWeb.Helpers;

public static class PasswordHelper
{
    /// <summary>Returns a random 16-byte hex salt.</summary>
    public static string GenerateSalt()
        => Convert.ToHexString(RandomNumberGenerator.GetBytes(16));

    /// <summary>SHA-256(salt + password), returned as lowercase hex.</summary>
    public static string HashPassword(string salt, string password)
    {
        var input = Encoding.UTF8.GetBytes(salt + password);
        var hash  = SHA256.HashData(input);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static bool Verify(string salt, string password, string storedHash)
        => HashPassword(salt, password) == storedHash;
}
