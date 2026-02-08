namespace Customers.IntegrationTests.Customers.Api.CustomerAggregateTests.Commands;

public class UpdateCustomerTests(WebApplicationFactory<Program> factory)
    : CustomersWebApplicationFixture(factory)
{
    [Fact]
    public async Task Update_WithValidData_Returns204AndPersistsChanges()
    {
        var created = await Client.CreateCustomerAsync();

        var request = new UpdateCustomer.Request(
            Name: "El Bar del Juanjo y la María",
            Slug: created.Slug,
            Description: "Nueva descripción",
            LogoUrl: "https://cdn.test.com/logo.jpg",
            EstablishmentType: "Restaurante",
            DefaultCulture: "es-ES",
            TimeZoneId: "Europe/Madrid",
            CuisineTypes: ["Española", "Mediterránea"],
            ServiceAmenities: ["Terraza"],
            DietaryOptions: ["Vegano"]);

        var response = await Client.PutAsJsonAsync("/customer", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Read-after-write
        var getResponse = await Client.GetAsync("/customer");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var persisted = await getResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        persisted!.Name.Should().Be("El Bar del Juanjo y la María");
        persisted.Description.Should().Be("Nueva descripción");
        persisted.CuisineTypes.Should().Contain("Española");
        persisted.ServiceAmenities.Should().Contain("Terraza");
    }

    [Fact]
    public async Task Update_WithoutCustomer_Returns404()
    {
        var request = new UpdateCustomer.Request(
            Name: "Test",
            Slug: "test-slug",
            Description: null,
            LogoUrl: null,
            EstablishmentType: "Bar",
            DefaultCulture: "es-ES",
            TimeZoneId: "Europe/Madrid",
            CuisineTypes: [],
            ServiceAmenities: [],
            DietaryOptions: []);

        var response = await Client.PutAsJsonAsync("/customer", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WithDuplicateSlug_Returns409()
    {
        var rivalClient = _factory.CreateRivalClient();
        var rival = await rivalClient.CreateCustomerAsync();

        await Client.CreateCustomerAsync();

        var request = new UpdateCustomer.Request(
            Name: "Test",
            Slug: rival.Slug,
            Description: null,
            LogoUrl: null,
            EstablishmentType: "Bar",
            DefaultCulture: "es-ES",
            TimeZoneId: "Europe/Madrid",
            CuisineTypes: [],
            ServiceAmenities: [],
            DietaryOptions: []);

        var response = await Client.PutAsJsonAsync("/customer", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Update_WithInvalidData_Returns422()
    {
        await Client.CreateCustomerAsync();

        var request = new UpdateCustomer.Request(
            Name: "",
            Slug: "test-slug",
            Description: null,
            LogoUrl: null,
            EstablishmentType: "Bar",
            DefaultCulture: "es-ES",
            TimeZoneId: "Europe/Madrid",
            CuisineTypes: [],
            ServiceAmenities: [],
            DietaryOptions: []);

        var response = await Client.PutAsJsonAsync("/customer", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
