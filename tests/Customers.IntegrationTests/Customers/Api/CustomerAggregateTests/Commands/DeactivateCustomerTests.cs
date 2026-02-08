namespace Customers.IntegrationTests.Customers.Api.CustomerAggregateTests.Commands;

public class DeactivateCustomerTests(WebApplicationFactory<Program> factory)
    : CustomersWebApplicationFixture(factory)
{
    [Fact]
    public async Task Deactivate_WithActiveCustomer_Returns200AndPersistsChanges()
    {
        await Client.CreateActiveCustomerAsync();

        var response = await Client.PostAsync("/customer/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Read-after-write
        var getResponse = await Client.GetAsync("/customer");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var persisted = await getResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        persisted!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Deactivate_WithoutCustomer_Returns404()
    {
        var response = await Client.PostAsync("/customer/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deactivate_WhenAlreadyInactive_Returns409()
    {
        await Client.CreateCustomerAsync();

        var response = await Client.PostAsync("/customer/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
