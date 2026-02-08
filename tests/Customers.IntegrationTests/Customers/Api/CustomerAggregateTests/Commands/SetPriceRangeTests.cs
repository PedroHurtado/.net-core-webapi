namespace Customers.IntegrationTests.Customers.Api.CustomerAggregateTests.Commands;

public class SetPriceRangeTests(WebApplicationFactory<Program> factory)
    : CustomersWebApplicationFixture(factory)
{
    [Fact]
    public async Task SetPriceRange_WithValidData_Returns204AndPersistsChanges()
    {
        await Client.CreateCustomerAsync();

        var request = new SetPriceRange.Request(
            MinPrice: 10m,
            MaxPrice: 30m);

        var response = await Client.PutAsJsonAsync("/customer/price-range", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Read-after-write
        var getResponse = await Client.GetAsync("/customer");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var persisted = await getResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        persisted!.HasPriceRange.Should().BeTrue();
        persisted.PriceRange!.MinPrice.Should().Be(10m);
        persisted.PriceRange.MaxPrice.Should().Be(30m);
    }

    [Fact]
    public async Task SetPriceRange_WithoutCustomer_Returns404()
    {
        var request = new SetPriceRange.Request(
            MinPrice: 10m,
            MaxPrice: 30m);

        var response = await Client.PutAsJsonAsync("/customer/price-range", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SetPriceRange_WithInvalidData_Returns422()
    {
        await Client.CreateCustomerAsync();

        var request = new SetPriceRange.Request(
            MinPrice: -5m,
            MaxPrice: 30m);

        var response = await Client.PutAsJsonAsync("/customer/price-range", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
