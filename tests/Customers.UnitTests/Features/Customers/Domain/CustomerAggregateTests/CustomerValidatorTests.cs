namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests;

public class CustomerValidatorTests : IClassFixture<DomainFixture>
{
    private readonly IValidator<Customer> _validator;

    public CustomerValidatorTests(DomainFixture fixture)
    {
        _validator = fixture.Get<IValidator<Customer>>();
    }

    private static TestableCustomer CreateValidCustomer()
    {
        var address = new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", new GeoPoint(38.0389m, -1.4917m));
        var contactInfo = new ContactInfo("639079481", null, null);
        var billingInfo = new BillingInfo("Bar Juanjo SL", "B12345678", address);

        return new TestableCustomer(Guid.NewGuid())
            .WithName("El Bar del Juanjo")
            .WithSlug("el-bar-del-juanjo")
            .WithEstablishmentType("Bar")
            .WithDefaultCulture("es-ES")
            .WithTimeZoneId("Europe/Madrid")
            .WithAddress(address)
            .WithContactInfo(contactInfo)
            .WithBillingInfo(billingInfo);
    }

    [Fact]
    public void Validate_WithValidCustomer_ShouldNotHaveErrors()
    {
        // Arrange
        var customer = CreateValidCustomer();

        // Act
        var result = _validator.Validate(customer);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    // --- Id ---

    [Fact]
    public void Validate_WithEmptyId_ShouldHaveError()
    {
        // Arrange
        var customer = CreateValidCustomer();
        var invalid = new TestableCustomer(Guid.Empty)
            .WithName(customer.Name)
            .WithSlug(customer.Slug)
            .WithEstablishmentType(customer.EstablishmentType)
            .WithDefaultCulture(customer.DefaultCulture)
            .WithTimeZoneId(customer.TimeZoneId)
            .WithAddress(customer.Address)
            .WithContactInfo(customer.ContactInfo)
            .WithBillingInfo(customer.BillingInfo);

        // Act
        var result = _validator.Validate(invalid);

        // Assert
        result.Errors.Should().Contain(x => x.ErrorMessage == CustomerValidationMessages.IdRequired);
    }

    // --- Name ---

    [Fact]
    public void Validate_WithEmptyName_ShouldHaveError()
    {
        // Arrange
        var customer = CreateValidCustomer().WithName(string.Empty);

        // Act
        var result = _validator.Validate(customer);

        // Assert
        result.Errors.Should().Contain(x => x.ErrorMessage == CustomerValidationMessages.NameRequired);
    }

    [Fact]
    public void Validate_WithNameExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var customer = CreateValidCustomer().WithName(new string('a', 151));

        // Act
        var result = _validator.Validate(customer);

        // Assert
        result.Errors.Should().Contain(x => x.ErrorMessage == CustomerValidationMessages.NameMaxLength);
    }

    // --- Slug ---

    [Fact]
    public void Validate_WithEmptySlug_ShouldHaveError()
    {
        // Arrange
        var customer = CreateValidCustomer().WithSlug(string.Empty);

        // Act
        var result = _validator.Validate(customer);

        // Assert
        result.Errors.Should().Contain(x => x.ErrorMessage == CustomerValidationMessages.SlugRequired);
    }

    [Fact]
    public void Validate_WithSlugExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var customer = CreateValidCustomer().WithSlug(new string('a', 151));

        // Act
        var result = _validator.Validate(customer);

        // Assert
        result.Errors.Should().Contain(x => x.ErrorMessage == CustomerValidationMessages.SlugMaxLength);
    }

    [Theory]
    [InlineData("El Bar del Juanjo")]
    [InlineData("UPPERCASE")]
    [InlineData("with spaces")]
    [InlineData("-starts-with-hyphen")]
    [InlineData("ends-with-hyphen-")]
    public void Validate_WithInvalidSlugFormat_ShouldHaveError(string slug)
    {
        // Arrange
        var customer = CreateValidCustomer().WithSlug(slug);

        // Act
        var result = _validator.Validate(customer);

        // Assert
        result.Errors.Should().Contain(x => x.ErrorMessage == CustomerValidationMessages.SlugFormat);
    }

    // --- Description ---

    [Fact]
    public void Validate_WithDescriptionExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var customer = CreateValidCustomer().WithDescription(new string('a', 2001));

        // Act
        var result = _validator.Validate(customer);

        // Assert
        result.Errors.Should().Contain(x => x.ErrorMessage == CustomerValidationMessages.DescriptionMaxLength);
    }

    [Fact]
    public void Validate_WithNullDescription_ShouldNotHaveError()
    {
        // Arrange
        var customer = CreateValidCustomer().WithDescription(null);

        // Act
        var result = _validator.Validate(customer);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    // --- LogoUrl ---

    [Fact]
    public void Validate_WithLogoUrlExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var customer = CreateValidCustomer().WithLogoUrl("https://cdn.fudie.com/" + new string('a', 500));

        // Act
        var result = _validator.Validate(customer);

        // Assert
        result.Errors.Should().Contain(x => x.ErrorMessage == CustomerValidationMessages.LogoUrlMaxLength);
    }

    [Fact]
    public void Validate_WithInvalidLogoUrl_ShouldHaveError()
    {
        // Arrange
        var customer = CreateValidCustomer().WithLogoUrl("not-a-url");

        // Act
        var result = _validator.Validate(customer);

        // Assert
        result.Errors.Should().Contain(x => x.ErrorMessage == CustomerValidationMessages.LogoUrlFormat);
    }

    [Fact]
    public void Validate_WithNullLogoUrl_ShouldNotHaveError()
    {
        // Arrange
        var customer = CreateValidCustomer().WithLogoUrl(null);

        // Act
        var result = _validator.Validate(customer);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    // --- EstablishmentType ---

    [Fact]
    public void Validate_WithEmptyEstablishmentType_ShouldHaveError()
    {
        // Arrange
        var customer = CreateValidCustomer().WithEstablishmentType(string.Empty);

        // Act
        var result = _validator.Validate(customer);

        // Assert
        result.Errors.Should().Contain(x => x.ErrorMessage == CustomerValidationMessages.EstablishmentTypeRequired);
    }

    [Fact]
    public void Validate_WithEstablishmentTypeExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var customer = CreateValidCustomer().WithEstablishmentType(new string('a', 101));

        // Act
        var result = _validator.Validate(customer);

        // Assert
        result.Errors.Should().Contain(x => x.ErrorMessage == CustomerValidationMessages.EstablishmentTypeMaxLength);
    }

    // --- DefaultCulture ---

    [Fact]
    public void Validate_WithEmptyDefaultCulture_ShouldHaveError()
    {
        // Arrange
        var customer = CreateValidCustomer().WithDefaultCulture(string.Empty);

        // Act
        var result = _validator.Validate(customer);

        // Assert
        result.Errors.Should().Contain(x => x.ErrorMessage == CustomerValidationMessages.DefaultCultureRequired);
    }

    [Theory]
    [InlineData("español")]
    [InlineData("es")]
    [InlineData("ES-es")]
    public void Validate_WithInvalidDefaultCultureFormat_ShouldHaveError(string culture)
    {
        // Arrange
        var customer = CreateValidCustomer().WithDefaultCulture(culture);

        // Act
        var result = _validator.Validate(customer);

        // Assert
        result.Errors.Should().Contain(x => x.ErrorMessage == CustomerValidationMessages.DefaultCultureFormat);
    }

    // --- TimeZoneId ---

    [Fact]
    public void Validate_WithEmptyTimeZoneId_ShouldHaveError()
    {
        // Arrange
        var customer = CreateValidCustomer().WithTimeZoneId(string.Empty);

        // Act
        var result = _validator.Validate(customer);

        // Assert
        result.Errors.Should().Contain(x => x.ErrorMessage == CustomerValidationMessages.TimeZoneRequired);
    }

    [Fact]
    public void Validate_WithTimeZoneIdExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var customer = CreateValidCustomer().WithTimeZoneId(new string('a', 101));

        // Act
        var result = _validator.Validate(customer);

        // Assert
        result.Errors.Should().Contain(x => x.ErrorMessage == CustomerValidationMessages.TimeZoneMaxLength);
    }

    // --- Address ---

    [Fact]
    public void Validate_WithNullAddress_ShouldHaveError()
    {
        // Arrange
        var customer = CreateValidCustomer().WithAddress(null!);

        // Act
        var result = _validator.Validate(customer);

        // Assert
        result.Errors.Should().Contain(x => x.ErrorMessage == CustomerValidationMessages.AddressRequired);
    }

    // --- ContactInfo ---

    [Fact]
    public void Validate_WithNullContactInfo_ShouldHaveError()
    {
        // Arrange
        var customer = CreateValidCustomer().WithContactInfo(null!);

        // Act
        var result = _validator.Validate(customer);

        // Assert
        result.Errors.Should().Contain(x => x.ErrorMessage == CustomerValidationMessages.ContactInfoRequired);
    }

    // --- BillingInfo ---

    [Fact]
    public void Validate_WithNullBillingInfo_ShouldHaveError()
    {
        // Arrange
        var customer = CreateValidCustomer().WithBillingInfo(null!);

        // Act
        var result = _validator.Validate(customer);

        // Assert
        result.Errors.Should().Contain(x => x.ErrorMessage == CustomerValidationMessages.BillingInfoRequired);
    }
}
