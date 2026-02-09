namespace Auth.UnitTests.Features.Users.Domain.UserAggregateTests.ValueObjectsTests;

public class HashedPasswordTests
{
    [Theory]
    [InlineData("$2a$12$abc123")]
    [InlineData("$2a$12$xyz789")]
    public void Hash_SetsCorrectValue(string hash)
    {
        var vo = new HashedPassword(hash, "salt-value");

        vo.Hash.Should().Be(hash);
    }

    [Theory]
    [InlineData("random-salt-1")]
    [InlineData("random-salt-2")]
    public void Salt_SetsCorrectValue(string salt)
    {
        var vo = new HashedPassword("$2a$12$abc123", salt);

        vo.Salt.Should().Be(salt);
    }
}
