namespace Customers.IntegrationTests.Customers.Api.CustomerAggregateTests.Commands;

public class UpdateBillingInfoTests(WebApplicationFactory<Program> factory)
    : CustomersWebApplicationFixture(factory)
{
    [Fact]
    public async Task UpdateBillingInfo_WithValidData_Returns204AndPersistsChanges()
    {
        await Client.CreateCustomerAsync();

        var request = new UpdateBillingInfo.Request(
            BusinessName: "Juanjo y María SL",
            TaxId: "B87654321",
            BillingAddress: new UpdateBillingInfo.AddressRequest(
                Street: "C/ Nueva, 5",
                City: "Murcia",
                PostalCode: "30001",
                Region: "Murcia",
                Country: "España",
                Latitude: 37.9922m,
                Longitude: -1.1307m));

        var response = await Client.PutAsJsonAsync("/customer/billing-info", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Read-after-write
        var getResponse = await Client.GetAsync("/customer");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var persisted = await getResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        persisted!.BillingInfo.BusinessName.Should().Be("Juanjo y María SL");
        persisted.BillingInfo.TaxId.Should().Be("B87654321");
        persisted.BillingInfo.BillingAddress.Street.Should().Be("C/ Nueva, 5");
    }

    [Fact]
    public async Task UpdateBillingInfo_WithoutCustomer_Returns404()
    {
        var request = new UpdateBillingInfo.Request(
            BusinessName: "Test SL",
            TaxId: "B12345678",
            BillingAddress: new UpdateBillingInfo.AddressRequest(
                Street: "Calle, 1",
                City: "Murcia",
                PostalCode: "30001",
                Region: "Murcia",
                Country: "España",
                Latitude: 38m,
                Longitude: -1m));

        var response = await Client.PutAsJsonAsync("/customer/billing-info", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateBillingInfo_WithInvalidData_Returns422()
    {
        await Client.CreateCustomerAsync();

        var request = new UpdateBillingInfo.Request(
            BusinessName: "",
            TaxId: "B12345678",
            BillingAddress: new UpdateBillingInfo.AddressRequest(
                Street: "Calle, 1",
                City: "Murcia",
                PostalCode: "30001",
                Region: "Murcia",
                Country: "España",
                Latitude: 38m,
                Longitude: -1m));

        var response = await Client.PutAsJsonAsync("/customer/billing-info", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
