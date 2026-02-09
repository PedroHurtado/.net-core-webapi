namespace Auth.UnitTests.Features.Users.Domain.UserAggregateTests;

public class UserValidatorTests
{
    private readonly UserValidator _validator = new();

    [Fact]
    public void Validate_WithValidGoogleUser_ReturnsSuccess()
    {
        var user = new TestableUser(Guid.NewGuid())
            .WithProviderId("google|117234856923")
            .WithProvider(AuthProvider.Google)
            .WithEmail("pedro@example.com")
            .WithName("Pedro García")
            .WithPhone("+34666123456")
            .WithAvatarUrl("https://lh3.googleusercontent.com/photo.jpg")
            .WithPassword(null)
            .WithIsActive(true);

        var result = _validator.Validate(user);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidLocalUser_ReturnsSuccess()
    {
        var user = new TestableUser(Guid.NewGuid())
            .WithProviderId("local|superadmin-001")
            .WithProvider(AuthProvider.Local)
            .WithEmail("admin@fudie.app")
            .WithName("Fudie Admin")
            .WithPassword(new HashedPassword("$2a$12$abc123", "random-salt"))
            .WithIsActive(true);

        var result = _validator.Validate(user);

        result.IsValid.Should().BeTrue();
    }

    #region Id Validation

    [Fact]
    public void Id_WhenEmpty_ReturnsError()
    {
        var user = new TestableUser(Guid.Empty)
            .WithProviderId("google|123")
            .WithProvider(AuthProvider.Google)
            .WithEmail("test@test.com")
            .WithName("Test")
            .WithIsActive(true);

        var result = _validator.Validate(user);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == UserValidationMessages.IdRequired);
    }

    #endregion

    #region ProviderId Validation

    [Fact]
    public void ProviderId_WhenEmpty_ReturnsError()
    {
        var user = new TestableUser(Guid.NewGuid())
            .WithProviderId("")
            .WithProvider(AuthProvider.Google)
            .WithEmail("test@test.com")
            .WithName("Test")
            .WithIsActive(true);

        var result = _validator.Validate(user);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == UserValidationMessages.ProviderIdRequired);
    }

    #endregion

    #region Email Validation

    [Fact]
    public void Email_WhenEmpty_ReturnsError()
    {
        var user = new TestableUser(Guid.NewGuid())
            .WithProviderId("google|123")
            .WithProvider(AuthProvider.Google)
            .WithEmail("")
            .WithName("Test")
            .WithIsActive(true);

        var result = _validator.Validate(user);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == UserValidationMessages.EmailRequired);
    }

    [Fact]
    public void Email_WhenExceedsMaxLength_ReturnsError()
    {
        var user = new TestableUser(Guid.NewGuid())
            .WithProviderId("google|123")
            .WithProvider(AuthProvider.Google)
            .WithEmail(new string('a', 245) + "@example.com")
            .WithName("Test")
            .WithIsActive(true);

        var result = _validator.Validate(user);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == UserValidationMessages.EmailMaxLength);
    }

    [Fact]
    public void Email_WhenInvalidFormat_ReturnsError()
    {
        var user = new TestableUser(Guid.NewGuid())
            .WithProviderId("google|123")
            .WithProvider(AuthProvider.Google)
            .WithEmail("not-an-email")
            .WithName("Test")
            .WithIsActive(true);

        var result = _validator.Validate(user);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == UserValidationMessages.EmailNotValid);
    }

    #endregion

    #region Name Validation

    [Fact]
    public void Name_WhenEmpty_ReturnsError()
    {
        var user = new TestableUser(Guid.NewGuid())
            .WithProviderId("google|123")
            .WithProvider(AuthProvider.Google)
            .WithEmail("test@test.com")
            .WithName("")
            .WithIsActive(true);

        var result = _validator.Validate(user);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == UserValidationMessages.NameRequired);
    }

    [Fact]
    public void Name_WhenExceedsMaxLength_ReturnsError()
    {
        var user = new TestableUser(Guid.NewGuid())
            .WithProviderId("google|123")
            .WithProvider(AuthProvider.Google)
            .WithEmail("test@test.com")
            .WithName(new string('a', 201))
            .WithIsActive(true);

        var result = _validator.Validate(user);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == UserValidationMessages.NameMaxLength);
    }

    #endregion

    #region Phone Validation

    [Fact]
    public void Phone_WhenExceedsMaxLength_ReturnsError()
    {
        var user = new TestableUser(Guid.NewGuid())
            .WithProviderId("google|123")
            .WithProvider(AuthProvider.Google)
            .WithEmail("test@test.com")
            .WithName("Test")
            .WithPhone(new string('1', 21))
            .WithIsActive(true);

        var result = _validator.Validate(user);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == UserValidationMessages.PhoneMaxLength);
    }

    [Fact]
    public void Phone_WhenNull_ReturnsSuccess()
    {
        var user = new TestableUser(Guid.NewGuid())
            .WithProviderId("google|123")
            .WithProvider(AuthProvider.Google)
            .WithEmail("test@test.com")
            .WithName("Test")
            .WithPhone(null)
            .WithIsActive(true);

        var result = _validator.Validate(user);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region AvatarUrl Validation

    [Fact]
    public void AvatarUrl_WhenExceedsMaxLength_ReturnsError()
    {
        var user = new TestableUser(Guid.NewGuid())
            .WithProviderId("google|123")
            .WithProvider(AuthProvider.Google)
            .WithEmail("test@test.com")
            .WithName("Test")
            .WithAvatarUrl(new string('a', 501))
            .WithIsActive(true);

        var result = _validator.Validate(user);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == UserValidationMessages.AvatarUrlMaxLength);
    }

    [Fact]
    public void AvatarUrl_WhenNull_ReturnsSuccess()
    {
        var user = new TestableUser(Guid.NewGuid())
            .WithProviderId("google|123")
            .WithProvider(AuthProvider.Google)
            .WithEmail("test@test.com")
            .WithName("Test")
            .WithAvatarUrl(null)
            .WithIsActive(true);

        var result = _validator.Validate(user);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Password Validation

    [Fact]
    public void Password_WhenNullForLocalUser_ReturnsError()
    {
        var user = new TestableUser(Guid.NewGuid())
            .WithProviderId("local|superadmin-001")
            .WithProvider(AuthProvider.Local)
            .WithEmail("admin@fudie.app")
            .WithName("Fudie Admin")
            .WithPassword(null)
            .WithIsActive(true);

        var result = _validator.Validate(user);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == UserValidationMessages.PasswordRequiredForLocal);
    }

    [Fact]
    public void Password_WhenSetForGoogleUser_ReturnsError()
    {
        var user = new TestableUser(Guid.NewGuid())
            .WithProviderId("google|123")
            .WithProvider(AuthProvider.Google)
            .WithEmail("test@test.com")
            .WithName("Test")
            .WithPassword(new HashedPassword("$2a$12$abc123", "random-salt"))
            .WithIsActive(true);

        var result = _validator.Validate(user);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == UserValidationMessages.PasswordOnlyForLocal);
    }

    #endregion
}
