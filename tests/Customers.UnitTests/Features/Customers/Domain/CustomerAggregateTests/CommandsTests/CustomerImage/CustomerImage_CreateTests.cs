namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.CommandsTests;

public class CustomerImageCreateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly CustomerImage.Create _create = fixture.Get<CustomerImage.Create>();

    [Fact]
    public void Execute_WithAllFields_ReturnsCustomerImage()
    {
        var command = new CreateCustomerImageCommand(
            "https://cdn.fudie.com/images/fachada.jpg",
            "Fachada del customere",
            1,
            true);

        var result = _create.Execute(command);

        result.Id.Should().NotBeEmpty();
        result.Url.Should().Be("https://cdn.fudie.com/images/fachada.jpg");
        result.AltText.Should().Be("Fachada del customere");
        result.DisplayOrder.Should().Be(1);
        result.IsCover.Should().BeTrue();
    }

    [Fact]
    public void Execute_WithUrlOnly_ReturnsCustomerImageWithDefaults()
    {
        var command = new CreateCustomerImageCommand("https://cdn.fudie.com/images/interior.jpg");

        var result = _create.Execute(command);

        result.Id.Should().NotBeEmpty();
        result.Url.Should().Be("https://cdn.fudie.com/images/interior.jpg");
        result.AltText.Should().BeNull();
        result.DisplayOrder.Should().Be(0);
        result.IsCover.Should().BeFalse();
    }

    [Fact]
    public void Execute_WithEmptyUrl_ThrowsValidationException()
    {
        var command = new CreateCustomerImageCommand("");

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{CustomerImageValidationMessages.UrlRequired}*");
    }

    [Fact]
    public void Execute_WithInvalidUrl_ThrowsValidationException()
    {
        var command = new CreateCustomerImageCommand("not-a-url");

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{CustomerImageValidationMessages.UrlFormat}*");
    }

    [Fact]
    public void Execute_WithAltTextExceedingMaxLength_ThrowsValidationException()
    {
        var command = new CreateCustomerImageCommand(
            "https://cdn.fudie.com/images/fachada.jpg",
            new string('a', 201));

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{CustomerImageValidationMessages.AltTextMaxLength}*");
    }

    [Fact]
    public void Execute_WithNegativeDisplayOrder_ThrowsValidationException()
    {
        var command = new CreateCustomerImageCommand(
            "https://cdn.fudie.com/images/fachada.jpg",
            null,
            -1);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{CustomerImageValidationMessages.DisplayOrderNonNegative}*");
    }
}
