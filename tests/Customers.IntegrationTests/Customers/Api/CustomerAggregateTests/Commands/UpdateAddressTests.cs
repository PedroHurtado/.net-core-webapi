namespace Customers.IntegrationTests.Customers.Api.CustomerAggregateTests.Commands;

public class UpdateAddressTests(WebApplicationFactory<Program> factory)
    : CustomersWebApplicationFixture(factory)
{
    [Fact]
    public async Task UpdateAddress_WithValidData_Returns204AndPersistsChanges()
    {
        await Client.CreateCustomerAsync();

        var request = new UpdateAddress.Request(
            Street: "Ctra. Murcia, 25",
            City: "La Puebla de Mula",
            PostalCode: "30193",
            Region: "Murcia",
            Country: "España",
            Latitude: 38.0390m,
            Longitude: -1.4918m);

        var response = await Client.PutAsJsonAsync("/customer/address", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Read-after-write
        var getResponse = await Client.GetAsync("/customer");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var persisted = await getResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        persisted!.Address.Street.Should().Be("Ctra. Murcia, 25");
        persisted.Address.Location.Latitude.Should().Be(38.0390m);
    }

    [Fact]
    public async Task UpdateAddress_WithoutCustomer_Returns404()
    {
        var request = new UpdateAddress.Request(
            Street: "Calle, 1",
            City: "Murcia",
            PostalCode: "30001",
            Region: "Murcia",
            Country: "España",
            Latitude: 38m,
            Longitude: -1m);

        var response = await Client.PutAsJsonAsync("/customer/address", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateAddress_WithInvalidData_Returns422()
    {
        await Client.CreateCustomerAsync();

        var request = new UpdateAddress.Request(
            Street: "",
            City: "Murcia",
            PostalCode: "30001",
            Region: "Murcia",
            Country: "España",
            Latitude: 38m,
            Longitude: -1m);

        var response = await Client.PutAsJsonAsync("/customer/address", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
