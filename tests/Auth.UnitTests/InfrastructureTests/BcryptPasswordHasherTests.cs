namespace Auth.UnitTests.InfrastructureTests;

public class BcryptPasswordHasherTests
{
    private readonly BcryptPasswordHasher _hasher = new();

    [Fact]
    public void GenerateSalt_ReturnsNonEmptyString()
    {
        var salt = _hasher.GenerateSalt();

        salt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateSalt_ReturnsDifferentSaltsOnEachCall()
    {
        var salt1 = _hasher.GenerateSalt();
        var salt2 = _hasher.GenerateSalt();

        salt1.Should().NotBe(salt2);
    }

    [Fact]
    public void Hash_WithValidPassword_ReturnsNonEmptyHash()
    {
        var salt = _hasher.GenerateSalt();

        var hash = _hasher.Hash("SecureP@ss123", salt);

        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Hash_WithSamePasswordAndSalt_ReturnsSameHash()
    {
        var salt = _hasher.GenerateSalt();

        var hash1 = _hasher.Hash("SecureP@ss123", salt);
        var hash2 = _hasher.Hash("SecureP@ss123", salt);

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        var salt = _hasher.GenerateSalt();
        var hash = _hasher.Hash("SecureP@ss123", salt);

        var result = _hasher.Verify("SecureP@ss123", hash, salt);

        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_WithIncorrectPassword_ReturnsFalse()
    {
        var salt = _hasher.GenerateSalt();
        var hash = _hasher.Hash("SecureP@ss123", salt);

        var result = _hasher.Verify("WrongPassword", hash, salt);

        result.Should().BeFalse();
    }
}
