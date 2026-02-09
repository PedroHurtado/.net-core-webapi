namespace Auth.UnitTests.Features.Users.Domain.UserAggregateTests.ValueObjectsTests;

public class HashedPasswordValidatorTests
{
    private readonly HashedPasswordValidator _validator = new();

    [Fact]
    public void Validate_WithValidHashedPassword_ReturnsSuccess()
    {
        var vo = new HashedPassword("$2a$12$abc123", "random-salt");

        var result = _validator.Validate(vo);

        result.IsValid.Should().BeTrue();
    }

    #region Hash Validation

    [Fact]
    public void Hash_WhenEmpty_ReturnsError()
    {
        var vo = new HashedPassword("", "random-salt");

        var result = _validator.Validate(vo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == HashedPasswordValidationMessages.HashRequired);
    }

    #endregion

    #region Salt Validation

    [Fact]
    public void Salt_WhenEmpty_ReturnsError()
    {
        var vo = new HashedPassword("$2a$12$abc123", "");

        var result = _validator.Validate(vo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == HashedPasswordValidationMessages.SaltRequired);
    }

    #endregion
}
