namespace Customers.IntegrationTests.Customers.Api.CustomerAggregateTests.Commands;

public class ActivateCustomerTests(WebApplicationFactory<Program> factory)
    : CustomersWebApplicationFixture(factory)
{
    [Fact]
    public async Task Activate_WithCompleteProfile_Returns200AndPersistsChanges()
    {
        await Client.CreateCompleteCustomerAsync();

        var response = await Client.PostAsync("/customer/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Read-after-write
        var getResponse = await Client.GetAsync("/customer");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var persisted = await getResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        persisted!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Activate_WithoutCustomer_Returns404()
    {
        var response = await Client.PostAsync("/customer/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Activate_WhenAlreadyActive_Returns409()
    {
        await Client.CreateActiveCustomerAsync();

        var response = await Client.PostAsync("/customer/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Activate_WithIncompleteProfile_Returns422()
    {
        await Client.CreateCustomerAsync();

        var response = await Client.PostAsync("/customer/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
