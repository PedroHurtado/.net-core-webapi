namespace Auth.Infrastructure;

/// <summary>
/// BCrypt implementation of <see cref="IPasswordHasher"/>.
/// </summary>
[Injectable(ServiceLifetime.Singleton)]
public class BcryptPasswordHasher : IPasswordHasher
{
    public string GenerateSalt()
    {
        return BCrypt.Net.BCrypt.GenerateSalt(12);
    }

    public string Hash(string plainPassword, string salt)
    {
        return BCrypt.Net.BCrypt.HashPassword(plainPassword, salt);
    }

    public bool Verify(string plainPassword, string hash, string salt)
    {
        var computedHash = BCrypt.Net.BCrypt.HashPassword(plainPassword, salt);
        return computedHash == hash;
    }
}
