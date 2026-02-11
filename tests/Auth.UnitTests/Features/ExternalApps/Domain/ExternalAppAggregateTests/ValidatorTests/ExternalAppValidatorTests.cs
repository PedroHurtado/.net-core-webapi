namespace Auth.UnitTests.Features.ExternalApps.Domain.ExternalAppAggregateTests.ValidatorTests;

public class ExternalAppValidatorTests
{
    private readonly ExternalAppValidator _validator = new();

    [Fact]
    public void Validate_WithValidExternalApp_ReturnsSuccess()
    {
        var externalApp = CreateValidExternalApp();

        var result = _validator.Validate(externalApp);

        result.IsValid.Should().BeTrue();
    }

    #region Id Validation

    [Fact]
    public void Id_WhenEmpty_ReturnsError()
    {
        var externalApp = CreateValidExternalApp(id: Guid.Empty);

        var result = _validator.Validate(externalApp);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ExternalAppValidationMessages.IdRequired);
    }

    #endregion

    #region TenantId Validation

    [Fact]
    public void TenantId_WhenEmpty_ReturnsError()
    {
        var externalApp = CreateValidExternalApp()
            .WithTenantId(Guid.Empty);

        var result = _validator.Validate(externalApp);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ExternalAppValidationMessages.TenantIdRequired);
    }

    #endregion

    #region Name Validation

    [Fact]
    public void Name_WhenEmpty_ReturnsError()
    {
        var externalApp = CreateValidExternalApp()
            .WithName("");

        var result = _validator.Validate(externalApp);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ExternalAppValidationMessages.NameRequired);
    }

    [Fact]
    public void Name_WhenExceedsMaxLength_ReturnsError()
    {
        var externalApp = CreateValidExternalApp()
            .WithName(new string('a', 151));

        var result = _validator.Validate(externalApp);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ExternalAppValidationMessages.NameMaxLength);
    }

    #endregion

    #region InvitationEmail Validation

    [Fact]
    public void InvitationEmail_WhenEmpty_ReturnsError()
    {
        var externalApp = CreateValidExternalApp()
            .WithInvitationEmail("");

        var result = _validator.Validate(externalApp);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ExternalAppValidationMessages.InvitationEmailRequired);
    }

    [Fact]
    public void InvitationEmail_WhenExceedsMaxLength_ReturnsError()
    {
        var externalApp = CreateValidExternalApp()
            .WithInvitationEmail(new string('a', 244) + "@ejemplo.com");

        var result = _validator.Validate(externalApp);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ExternalAppValidationMessages.InvitationEmailMaxLength);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    public void InvitationEmail_WhenInvalidFormat_ReturnsError(string email)
    {
        var externalApp = CreateValidExternalApp()
            .WithInvitationEmail(email);

        var result = _validator.Validate(externalApp);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ExternalAppValidationMessages.InvitationEmailInvalidFormat);
    }

    [Theory]
    [InlineData("dev@misoftware.com")]
    [InlineData("user@domain.co")]
    public void InvitationEmail_WhenValid_ReturnsSuccess(string email)
    {
        var externalApp = CreateValidExternalApp()
            .WithInvitationEmail(email);

        var result = _validator.Validate(externalApp);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region InvitationStatus Validation

    [Fact]
    public void InvitationStatus_WhenInvalid_ReturnsError()
    {
        var externalApp = CreateValidExternalApp()
            .WithInvitationStatus((InvitationStatus)99);

        var result = _validator.Validate(externalApp);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ExternalAppValidationMessages.InvitationStatusInvalid);
    }

    #endregion

    #region ApiKeyPrefix Validation

    [Fact]
    public void ApiKeyPrefix_WhenExceedsMaxLength_ReturnsError()
    {
        var externalApp = CreateValidExternalApp()
            .WithApiKeyPrefix(new string('a', 9));

        var result = _validator.Validate(externalApp);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ExternalAppValidationMessages.ApiKeyPrefixMaxLength);
    }

    [Fact]
    public void ApiKeyPrefix_WhenNull_ReturnsSuccess()
    {
        var externalApp = CreateValidExternalApp()
            .WithApiKeyPrefix(null);

        var result = _validator.Validate(externalApp);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ApiKeyPrefix_WhenValidLength_ReturnsSuccess()
    {
        var externalApp = CreateValidExternalApp()
            .WithApiKeyPrefix("fud_a3K9");

        var result = _validator.Validate(externalApp);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    private static TestableExternalApp CreateValidExternalApp(Guid? id = null)
    {
        return new TestableExternalApp(id ?? Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithIsActive(true)
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Pending);
    }
}
