namespace Auth.UnitTests.Features.Memberships.Domain.MembershipAggregateTests.ValidatorTests;

public class MembershipValidatorTests
{
    private readonly MembershipValidator _validator = new();

    [Fact]
    public void Validate_WithValidMembership_ReturnsSuccess()
    {
        var membership = CreateValidMembership();

        var result = _validator.Validate(membership);

        result.IsValid.Should().BeTrue();
    }

    #region Id Validation

    [Fact]
    public void Id_WhenEmpty_ReturnsError()
    {
        var membership = CreateValidMembership(id: Guid.Empty);

        var result = _validator.Validate(membership);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MembershipValidationMessages.IdRequired);
    }

    #endregion

    #region TenantId Validation

    [Fact]
    public void TenantId_WhenEmpty_ReturnsError()
    {
        var membership = CreateValidMembership()
            .WithTenantId(Guid.Empty);

        var result = _validator.Validate(membership);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MembershipValidationMessages.TenantIdRequired);
    }

    #endregion

    #region Role Validation

    [Fact]
    public void Role_WhenNull_ReturnsError()
    {
        var membership = CreateValidMembership()
            .WithRole(null!);

        var result = _validator.Validate(membership);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MembershipValidationMessages.RoleRequired);
    }

    #endregion

    #region InvitationEmail Validation

    [Fact]
    public void InvitationEmail_WhenEmpty_ReturnsError()
    {
        var membership = CreateValidMembership()
            .WithInvitationEmail("");

        var result = _validator.Validate(membership);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MembershipValidationMessages.InvitationEmailRequired);
    }

    [Fact]
    public void InvitationEmail_WhenExceedsMaxLength_ReturnsError()
    {
        var membership = CreateValidMembership()
            .WithInvitationEmail(new string('a', 244) + "@ejemplo.com");

        var result = _validator.Validate(membership);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MembershipValidationMessages.InvitationEmailMaxLength);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    public void InvitationEmail_WhenInvalidFormat_ReturnsError(string email)
    {
        var membership = CreateValidMembership()
            .WithInvitationEmail(email);

        var result = _validator.Validate(membership);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MembershipValidationMessages.InvitationEmailInvalidFormat);
    }

    [Theory]
    [InlineData("maria@ejemplo.com")]
    [InlineData("user@domain.co")]
    public void InvitationEmail_WhenValid_ReturnsSuccess(string email)
    {
        var membership = CreateValidMembership()
            .WithInvitationEmail(email);

        var result = _validator.Validate(membership);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region InvitationStatus Validation

    [Fact]
    public void InvitationStatus_WhenInvalid_ReturnsError()
    {
        var membership = CreateValidMembership()
            .WithInvitationStatus((InvitationStatus)99);

        var result = _validator.Validate(membership);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MembershipValidationMessages.InvitationStatusInvalid);
    }

    #endregion

    private static TestableMembership CreateValidMembership(Guid? id = null)
    {
        return new TestableMembership(id ?? Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithRole(new TestableTenantRole(Guid.NewGuid()))
            .WithIsActive(true)
            .WithInvitationEmail("maria@ejemplo.com")
            .WithInvitationStatus(InvitationStatus.Pending);
    }
}
