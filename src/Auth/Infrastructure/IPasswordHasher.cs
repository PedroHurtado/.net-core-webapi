namespace Auth.Infrastructure;

/// <summary>
/// Abstracts the password hashing algorithm (BCrypt, Argon2, etc.).
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Generates a cryptographically secure random salt.
    /// </summary>
    /// <returns>The generated salt string.</returns>
    string GenerateSalt();

    /// <summary>
    /// Hashes a plain password using the provided salt.
    /// </summary>
    /// <param name="plainPassword">The plain text password.</param>
    /// <param name="salt">The salt to use for hashing.</param>
    /// <returns>The hashed password string.</returns>
    string Hash(string plainPassword, string salt);

    /// <summary>
    /// Verifies a plain password against a previously hashed password and salt.
    /// </summary>
    /// <param name="plainPassword">The plain text password to verify.</param>
    /// <param name="hash">The stored hash to compare against.</param>
    /// <param name="salt">The salt used during the original hashing.</param>
    /// <returns><c>true</c> if the password matches; otherwise, <c>false</c>.</returns>
    bool Verify(string plainPassword, string hash, string salt);
}
