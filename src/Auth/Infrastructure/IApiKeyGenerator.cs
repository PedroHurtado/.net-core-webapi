namespace Auth.Infrastructure;

/// <summary>
/// Result of generating a new API key.
/// </summary>
/// <param name="RawKey">The raw API key to return to the caller (shown once).</param>
/// <param name="Hash">The BCrypt hash of the API key for storage.</param>
/// <param name="Salt">The BCrypt salt used for hashing.</param>
/// <param name="Prefix">The first 8 characters of the API key for identification.</param>
public record ApiKeyResult(string RawKey, string Hash, string Salt, string Prefix);

/// <summary>
/// Generates and verifies API keys using a secure hashing algorithm.
/// </summary>
public interface IApiKeyGenerator
{
    /// <summary>
    /// Generates a new API key with its hash, salt, and prefix.
    /// </summary>
    /// <returns>An <see cref="ApiKeyResult"/> containing the raw key and its hashed components.</returns>
    ApiKeyResult Generate();

    /// <summary>
    /// Verifies a raw API key against a stored hash and salt.
    /// </summary>
    /// <param name="rawKey">The raw API key to verify.</param>
    /// <param name="storedHash">The stored BCrypt hash.</param>
    /// <param name="storedSalt">The stored BCrypt salt.</param>
    /// <returns><c>true</c> if the key matches; otherwise, <c>false</c>.</returns>
    bool Verify(string rawKey, string storedHash, string storedSalt);
}
