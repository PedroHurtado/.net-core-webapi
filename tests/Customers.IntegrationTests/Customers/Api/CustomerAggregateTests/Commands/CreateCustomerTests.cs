namespace Customers.IntegrationTests.Customers.Api.CustomerAggregateTests.Commands;

public class CreateCustomerTests(WebApplicationFactory<Program> factory)
    : CustomersWebApplicationFixture(factory)
{
    [Fact]
    public async Task Create_WithValidData_Returns201AndPersistsData()
    {
        var slug = $"el-bar-del-juanjo-{Guid.NewGuid():N}";

        var request = new CreateCustomer.Request(
            Id: Guid.NewGuid(),
            Name: "El Bar del Juanjo",
            Slug: slug,
            Description: "Un bar con encanto",
            EstablishmentType: "Bar",
            DefaultCulture: "es-ES",
            TimeZoneId: "Europe/Madrid",
            Address: new CreateCustomer.CreateAddressRequest(
                Street: "Ctra. Murcia, 23",
                City: "La Puebla de Mula",
                PostalCode: "30193",
                Region: "Murcia",
                Country: "España",
                Latitude: 38.0389m,
                Longitude: -1.4917m),
            ContactInfo: new CreateCustomer.CreateContactInfoRequest(
                Phone: "639079481",
                Email: "juanjo@example.com",
                WebsiteUrl: null),
            BillingInfo: new CreateCustomer.CreateBillingInfoRequest(
                BusinessName: "Bar Juanjo SL",
                TaxId: "B12345678",
                BillingAddress: new CreateCustomer.CreateAddressRequest(
                    Street: "Ctra. Murcia, 23",
                    City: "La Puebla de Mula",
                    PostalCode: "30193",
                    Region: "Murcia",
                    Country: "España",
                    Latitude: 38.0389m,
                    Longitude: -1.4917m)));

        var response = await Client.PostAsJsonAsync("/customer", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Read-after-write
        var getResponse = await Client.GetAsync("/customer");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var persisted = await getResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        persisted!.Name.Should().Be("El Bar del Juanjo");
        persisted.Slug.Should().Be(slug);
        persisted.IsActive.Should().BeFalse();
        persisted.Address.Street.Should().Be("Ctra. Murcia, 23");
        persisted.ContactInfo.Phone.Should().Be("639079481");
        persisted.BillingInfo.BusinessName.Should().Be("Bar Juanjo SL");
    }

    [Fact]
    public async Task Create_WithDuplicateSlug_Returns409()
    {
        var created = await Client.CreateCustomerAsync();

        var request = new CreateCustomer.Request(
            Id: Guid.NewGuid(),
            Name: "Otro Bar",
            Slug: created.Slug,
            Description: null,
            EstablishmentType: "Bar",
            DefaultCulture: "es-ES",
            TimeZoneId: "Europe/Madrid",
            Address: new CreateCustomer.CreateAddressRequest(
                Street: "Otra calle, 1",
                City: "Murcia",
                PostalCode: "30001",
                Region: "Murcia",
                Country: "España",
                Latitude: 37.9922m,
                Longitude: -1.1307m),
            ContactInfo: new CreateCustomer.CreateContactInfoRequest(
                Phone: "600000000",
                Email: null,
                WebsiteUrl: null),
            BillingInfo: new CreateCustomer.CreateBillingInfoRequest(
                BusinessName: "Otro SL",
                TaxId: "B99999999",
                BillingAddress: new CreateCustomer.CreateAddressRequest(
                    Street: "Otra calle, 1",
                    City: "Murcia",
                    PostalCode: "30001",
                    Region: "Murcia",
                    Country: "España",
                    Latitude: 37.9922m,
                    Longitude: -1.1307m)));

        var response = await Client.PostAsJsonAsync("/customer", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_WithInvalidData_Returns422()
    {
        var request = new CreateCustomer.Request(
            Id: Guid.NewGuid(),
            Name: "",
            Slug: $"valid-slug-{Guid.NewGuid():N}",
            Description: null,
            EstablishmentType: "Bar",
            DefaultCulture: "es-ES",
            TimeZoneId: "Europe/Madrid",
            Address: new CreateCustomer.CreateAddressRequest(
                Street: "Calle, 1",
                City: "Murcia",
                PostalCode: "30001",
                Region: "Murcia",
                Country: "España",
                Latitude: 38m,
                Longitude: -1m),
            ContactInfo: new CreateCustomer.CreateContactInfoRequest(
                Phone: "600000000",
                Email: null,
                WebsiteUrl: null),
            BillingInfo: new CreateCustomer.CreateBillingInfoRequest(
                BusinessName: "Test SL",
                TaxId: "B12345678",
                BillingAddress: new CreateCustomer.CreateAddressRequest(
                    Street: "Calle, 1",
                    City: "Murcia",
                    PostalCode: "30001",
                    Region: "Murcia",
                    Country: "España",
                    Latitude: 38m,
                    Longitude: -1m)));

        var response = await Client.PostAsJsonAsync("/customer", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
