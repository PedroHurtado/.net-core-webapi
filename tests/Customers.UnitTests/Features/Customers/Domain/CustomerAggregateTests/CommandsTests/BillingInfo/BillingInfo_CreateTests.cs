namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.CommandsTests;

public class BillingInfoCreateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly BillingInfo.Create _create = fixture.Get<BillingInfo.Create>();

    private static CreateAddressCommand ValidAddress => new(
        "Ctra. Murcia, 23",
        "La Puebla de Mula",
        "30193",
        "Murcia",
        "España",
        38.0389m,
        -1.4917m);

    [Fact]
    public void Execute_WithValidCommand_ReturnsBillingInfo()
    {
        var command = new CreateBillingInfoCommand("Bar Juanjo SL", "B12345678", ValidAddress);

        var result = _create.Execute(command);

        result.BusinessName.Should().Be("Bar Juanjo SL");
        result.TaxId.Should().Be("B12345678");
        result.BillingAddress.Street.Should().Be("Ctra. Murcia, 23");
    }

    [Fact]
    public void Execute_WithDifferentBillingAddress_ReturnsBillingInfo()
    {
        var billingAddress = new CreateAddressCommand("C/ Gran Vía, 1", "Murcia", "30001", "Murcia", "España", 37.9922m, -1.1307m);
        var command = new CreateBillingInfoCommand("Bar Juanjo SL", "B12345678", billingAddress);

        var result = _create.Execute(command);

        result.BillingAddress.Street.Should().Be("C/ Gran Vía, 1");
        result.BillingAddress.City.Should().Be("Murcia");
    }

    [Fact]
    public void Execute_WithEmptyBusinessName_ThrowsValidationException()
    {
        var command = new CreateBillingInfoCommand("", "B12345678", ValidAddress);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{BillingInfoValidationMessages.BusinessNameRequired}*");
    }

    [Fact]
    public void Execute_WithEmptyTaxId_ThrowsValidationException()
    {
        var command = new CreateBillingInfoCommand("Bar Juanjo SL", "", ValidAddress);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{BillingInfoValidationMessages.TaxIdRequired}*");
    }

    [Fact]
    public void Execute_WithInvalidBillingAddress_ThrowsValidationException()
    {
        var invalidAddress = new CreateAddressCommand("", "La Puebla de Mula", "30193", "Murcia", "España", 38.0389m, -1.4917m);
        var command = new CreateBillingInfoCommand("Bar Juanjo SL", "B12345678", invalidAddress);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{AddressValidationMessages.StreetRequired}*");
    }
}
