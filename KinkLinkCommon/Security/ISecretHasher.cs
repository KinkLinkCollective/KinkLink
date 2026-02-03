using System.Threading.Tasks;

namespace KinkLinkCommon.Security;

public interface ISecretHasher
{
    /// <summary>
    /// Hashes a secret key using SHA-256 with HMAC and a randomly generated salt
    /// </summary>
    /// <param name="secret">The plain text secret to hash</param>
    /// <returns>Tuple containing the hash and salt as base64 strings</returns>
    Task<(string Hash, string Salt)> HashSecretAsync(string secret);

    /// <summary>
    /// Verifies a plain text secret against a stored hash and salt
    /// </summary>
    /// <param name="secret">The plain text secret to verify</param>
    /// <param name="storedHash">The stored hash as base64 string</param>
    /// <param name="storedSalt">The stored salt as base64 string</param>
    /// <returns>True if the secret matches, false otherwise</returns>
    Task<bool> VerifySecretAsync(string secret, string storedHash, string storedSalt);
}