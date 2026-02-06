namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.ValueObjectsTests;

public class AddressValidatorTests
{
    private readonly AddressValidator _validator = new();
    private static readonly GeoPoint ValidLocation = new(38.0389m, -1.4917m);

    [Fact]
    public void Validate_WithValidAddress_ReturnsSuccess()
    {
        var address = new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", ValidLocation);

        var result = _validator.Validate(address);

        result.IsValid.Should().BeTrue();
    }

    #region Street Validation

    [Fact]
    public void Street_WhenEmpty_ReturnsError()
    {
        var address = new Address("", "La Puebla de Mula", "30193", "Murcia", "España", ValidLocation);

        var result = _validator.Validate(address);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == AddressValidationMessages.StreetRequired);
    }

    [Fact]
    public void Street_WhenExceedsMaxLength_ReturnsError()
    {
        var address = new Address(new string('a', 201), "La Puebla de Mula", "30193", "Murcia", "España", ValidLocation);

        var result = _validator.Validate(address);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == AddressValidationMessages.StreetMaxLength);
    }

    #endregion

    #region City Validation

    [Fact]
    public void City_WhenEmpty_ReturnsError()
    {
        var address = new Address("Ctra. Murcia, 23", "", "30193", "Murcia", "España", ValidLocation);

        var result = _validator.Validate(address);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == AddressValidationMessages.CityRequired);
    }

    [Fact]
    public void City_WhenExceedsMaxLength_ReturnsError()
    {
        var address = new Address("Ctra. Murcia, 23", new string('a', 101), "30193", "Murcia", "España", ValidLocation);

        var result = _validator.Validate(address);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == AddressValidationMessages.CityMaxLength);
    }

    #endregion

    #region PostalCode Validation

    [Fact]
    public void PostalCode_WhenEmpty_ReturnsError()
    {
        var address = new Address("Ctra. Murcia, 23", "La Puebla de Mula", "", "Murcia", "España", ValidLocation);

        var result = _validator.Validate(address);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == AddressValidationMessages.PostalCodeRequired);
    }

    [Fact]
    public void PostalCode_WhenExceedsMaxLength_ReturnsError()
    {
        var address = new Address("Ctra. Murcia, 23", "La Puebla de Mula", new string('a', 21), "Murcia", "España", ValidLocation);

        var result = _validator.Validate(address);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == AddressValidationMessages.PostalCodeMaxLength);
    }

    #endregion

    #region Region Validation

    [Fact]
    public void Region_WhenEmpty_ReturnsError()
    {
        var address = new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "", "España", ValidLocation);

        var result = _validator.Validate(address);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == AddressValidationMessages.RegionRequired);
    }

    [Fact]
    public void Region_WhenExceedsMaxLength_ReturnsError()
    {
        var address = new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", new string('a', 101), "España", ValidLocation);

        var result = _validator.Validate(address);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == AddressValidationMessages.RegionMaxLength);
    }

    #endregion

    #region Country Validation

    [Fact]
    public void Country_WhenEmpty_ReturnsError()
    {
        var address = new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "", ValidLocation);

        var result = _validator.Validate(address);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == AddressValidationMessages.CountryRequired);
    }

    [Fact]
    public void Country_WhenExceedsMaxLength_ReturnsError()
    {
        var address = new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", new string('a', 101), ValidLocation);

        var result = _validator.Validate(address);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == AddressValidationMessages.CountryMaxLength);
    }

    #endregion

    #region Location Validation

    [Fact]
    public void Location_WhenNull_ReturnsError()
    {
        var address = new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", null!);

        var result = _validator.Validate(address);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == AddressValidationMessages.LocationRequired);
    }

    #endregion
}
