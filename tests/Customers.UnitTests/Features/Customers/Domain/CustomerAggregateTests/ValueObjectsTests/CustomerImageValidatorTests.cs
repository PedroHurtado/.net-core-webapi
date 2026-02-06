namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.ValueObjectsTests;

public class CustomerImageValidatorTests
{
    private readonly CustomerImageValidator _validator = new();

    [Fact]
    public void Validate_WithValidCustomerImage_ReturnsSuccess()
    {
        var image = new CustomerImage(
            Guid.NewGuid(),
            "https://cdn.fudie.com/images/fachada.jpg",
            "Fachada del customer",
            1,
            true);

        var result = _validator.Validate(image);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidCustomerImage_MinimalFields_ReturnsSuccess()
    {
        var image = new CustomerImage(
            Guid.NewGuid(),
            "https://cdn.fudie.com/images/interior.jpg",
            null,
            0,
            false);

        var result = _validator.Validate(image);

        result.IsValid.Should().BeTrue();
    }

    #region Id Validation

    [Fact]
    public void Id_WhenEmpty_ReturnsError()
    {
        var image = new CustomerImage(
            Guid.Empty,
            "https://cdn.fudie.com/images/fachada.jpg",
            null,
            0,
            false);

        var result = _validator.Validate(image);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == CustomerImageValidationMessages.IdRequired);
    }

    #endregion

    #region Url Validation

    [Fact]
    public void Url_WhenEmpty_ReturnsError()
    {
        var image = new CustomerImage(Guid.NewGuid(), "", null, 0, false);

        var result = _validator.Validate(image);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == CustomerImageValidationMessages.UrlRequired);
    }

    [Fact]
    public void Url_WhenExceedsMaxLength_ReturnsError()
    {
        var image = new CustomerImage(Guid.NewGuid(), "https://" + new string('a', 493), null, 0, false);

        var result = _validator.Validate(image);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == CustomerImageValidationMessages.UrlMaxLength);
    }

    [Fact]
    public void Url_WhenInvalidFormat_ReturnsError()
    {
        var image = new CustomerImage(Guid.NewGuid(), "not-a-url", null, 0, false);

        var result = _validator.Validate(image);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == CustomerImageValidationMessages.UrlFormat);
    }

    #endregion

    #region AltText Validation

    [Fact]
    public void AltText_WhenExceedsMaxLength_ReturnsError()
    {
        var image = new CustomerImage(
            Guid.NewGuid(),
            "https://cdn.fudie.com/images/fachada.jpg",
            new string('a', 201),
            0,
            false);

        var result = _validator.Validate(image);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == CustomerImageValidationMessages.AltTextMaxLength);
    }

    #endregion

    #region DisplayOrder Validation

    [Fact]
    public void DisplayOrder_WhenNegative_ReturnsError()
    {
        var image = new CustomerImage(
            Guid.NewGuid(),
            "https://cdn.fudie.com/images/fachada.jpg",
            null,
            -1,
            false);

        var result = _validator.Validate(image);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == CustomerImageValidationMessages.DisplayOrderNonNegative);
    }

    #endregion
}
