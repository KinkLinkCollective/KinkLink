using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KinkLinkCommon.Security;

public class SecretHasher : ISecretHasher
{
    private const int SaltSize = 16; // 128-bit salt
    
    public async Task<(string Hash, string Salt)> HashSecretAsync(string secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
            throw new ArgumentException("Secret cannot be null or empty", nameof(secret));

        return await Task.Run(() =>
        {
            // Generate cryptographically secure salt
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            
            // Hash with HMAC-SHA256
            using var hmac = new HMACSHA256(salt);
            var secretBytes = Encoding.UTF8.GetBytes(secret);
            var hash = hmac.ComputeHash(secretBytes);
            
            // Convert to base64 for database storage
            var hashString = Convert.ToBase64String(hash);
            var saltString = Convert.ToBase64String(salt);
            
            return (hashString, saltString);
        });
    }
    
    public async Task<bool> VerifySecretAsync(string secret, string storedHash, string storedSalt)
    {
        if (string.IsNullOrWhiteSpace(secret))
            return false;
            
        if (string.IsNullOrWhiteSpace(storedHash) || string.IsNullOrWhiteSpace(storedSalt))
            return false;

        try
        {
            return await Task.Run(() =>
            {
                // Convert stored salt back to bytes
                var saltBytes = Convert.FromBase64String(storedSalt);
                
                // Compute hash of provided secret using stored salt
                using var hmac = new HMACSHA256(saltBytes);
                var secretBytes = Encoding.UTF8.GetBytes(secret);
                var computedHash = hmac.ComputeHash(secretBytes);
                
                // Compare with stored hash
                var storedHashBytes = Convert.FromBase64String(storedHash);
                
                // Use constant-time comparison to prevent timing attacks
                return CryptographicOperations.FixedTimeEquals(computedHash, storedHashBytes);
            });
        }
        catch (FormatException)
        {
            // Invalid base64 format
            return false;
        }
    }
}