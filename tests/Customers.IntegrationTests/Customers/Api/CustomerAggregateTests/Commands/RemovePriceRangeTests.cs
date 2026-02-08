namespace Customers.IntegrationTests.Customers.Api.CustomerAggregateTests.Commands;

public class RemovePriceRangeTests(WebApplicationFactory<Program> factory)
    : CustomersWebApplicationFixture(factory)
{
    [Fact]
    public async Task RemovePriceRange_WithExisting_Returns204AndPersistsChanges()
    {
        await Client.CreateCustomerWithPriceRangeAsync();

        var response = await Client.DeleteAsync("/customer/price-range");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Read-after-write
        var getResponse = await Client.GetAsync("/customer");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var persisted = await getResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        persisted!.HasPriceRange.Should().BeFalse();
        persisted.PriceRange.Should().BeNull();
    }

    [Fact]
    public async Task RemovePriceRange_WithoutCustomer_Returns404()
    {
        var response = await Client.DeleteAsync("/customer/price-range");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
