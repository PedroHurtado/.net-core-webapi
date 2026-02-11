namespace Auth.UnitTests.InfrastructureTests;

public class ApiKeyGeneratorTests
{
    private readonly ApiKeyGenerator _generator = new(new BcryptPasswordHasher());

    [Fact]
    public void Generate_ReturnsNonEmptyResult()
    {
        var result = _generator.Generate();

        result.RawKey.Should().NotBeNullOrEmpty();
        result.Hash.Should().NotBeNullOrEmpty();
        result.Salt.Should().NotBeNullOrEmpty();
        result.Prefix.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Generate_RawKeyStartsWithFudPrefix()
    {
        var result = _generator.Generate();

        result.RawKey.Should().StartWith("fud_");
    }

    [Fact]
    public void Generate_RawKeyHasCorrectLength()
    {
        var result = _generator.Generate();

        result.RawKey.Should().HaveLength(36);
    }

    [Fact]
    public void Generate_PrefixMatchesFirst8CharsOfRawKey()
    {
        var result = _generator.Generate();

        result.Prefix.Should().Be(result.RawKey[..8]);
    }

    [Fact]
    public void Generate_ReturnsDifferentKeysOnEachCall()
    {
        var result1 = _generator.Generate();
        var result2 = _generator.Generate();

        result1.RawKey.Should().NotBe(result2.RawKey);
    }

    [Fact]
    public void Verify_WithCorrectRawKey_ReturnsTrue()
    {
        var result = _generator.Generate();

        var verified = _generator.Verify(result.RawKey, result.Hash, result.Salt);

        verified.Should().BeTrue();
    }

    [Fact]
    public void Verify_WithIncorrectRawKey_ReturnsFalse()
    {
        var result = _generator.Generate();

        var verified = _generator.Verify("fud_wrongkey12345678901234567890ab", result.Hash, result.Salt);

        verified.Should().BeFalse();
    }
}
