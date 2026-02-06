namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.CommandsTests;

public class CustomerActivateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Customer.Activate _activate = fixture.Get<Customer.Activate>();

    [Fact]
    public void Execute_WithCompleteProfile_ActivatesCustomer()
    {
        var customer = CreateCompleteCustomer();

        var result = _activate.Execute(customer);

        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Execute_WhenAlreadyActive_ThrowsConflictException()
    {
        var customer = CreateCompleteCustomer()
            .WithIsActive(true);

        var act = () => _activate.Execute(customer);

        act.Should().Throw<ConflictException>()
            .WithMessage("*Customer is already active*");
    }

    [Fact]
    public void Execute_WithoutLogo_ThrowsValidationException()
    {
        var customer = CreateValidCustomer()
            .WithDescription("Descripción")
            .WithImage(new CustomerImage(Guid.NewGuid(), "https://cdn.fudie.com/images/fachada.jpg", "Fachada", 0, true))
            .WithCuisineType("Española");

        var act = () => _activate.Execute(customer);

        act.Should().Throw<ValidationException>()
            .WithMessage("*Customer profile must be complete before activation*");
    }

    [Fact]
    public void Execute_WithoutImages_ThrowsValidationException()
    {
        var customer = CreateValidCustomer()
            .WithDescription("Descripción")
            .WithLogoUrl("https://cdn.fudie.com/logo.jpg")
            .WithCuisineType("Española");

        var act = () => _activate.Execute(customer);

        act.Should().Throw<ValidationException>()
            .WithMessage("*Customer profile must be complete before activation*");
    }

    [Fact]
    public void Execute_WithoutCuisineTypes_ThrowsValidationException()
    {
        var customer = CreateValidCustomer()
            .WithDescription("Descripción")
            .WithLogoUrl("https://cdn.fudie.com/logo.jpg")
            .WithImage(new CustomerImage(Guid.NewGuid(), "https://cdn.fudie.com/images/fachada.jpg", "Fachada", 0, true));

        var act = () => _activate.Execute(customer);

        act.Should().Throw<ValidationException>()
            .WithMessage("*Customer profile must be complete before activation*");
    }

    private static TestableCustomer CreateCompleteCustomer() =>
        CreateValidCustomer()
            .WithDescription("Bar de tapas")
            .WithLogoUrl("https://cdn.fudie.com/logo.jpg")
            .WithImage(new CustomerImage(Guid.NewGuid(), "https://cdn.fudie.com/images/fachada.jpg", "Fachada", 0, true))
            .WithCuisineType("Española");

    private static TestableCustomer CreateValidCustomer() =>
        new TestableCustomer(Guid.NewGuid())
            .WithName("El Bar del Juanjo")
            .WithSlug("el-bar-del-juanjo")
            .WithEstablishmentType("Bar")
            .WithDefaultCulture("es-ES")
            .WithTimeZoneId("Europe/Madrid")
            .WithAddress(new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", new GeoPoint(38.0389m, -1.4917m)))
            .WithContactInfo(new ContactInfo("639079481", null, null))
            .WithBillingInfo(new BillingInfo("Bar Juanjo SL", "B12345678", new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", new GeoPoint(38.0389m, -1.4917m))));
}
