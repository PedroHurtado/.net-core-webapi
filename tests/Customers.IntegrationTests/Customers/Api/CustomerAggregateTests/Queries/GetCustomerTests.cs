namespace Customers.IntegrationTests.Customers.Api.CustomerAggregateTests.Queries;

public class GetCustomerTests(WebApplicationFactory<Program> factory)
    : CustomersWebApplicationFixture(factory)
{
    [Fact]
    public async Task Get_WithExistingCustomer_Returns200WithData()
    {
        var created = await Client.CreateCustomerAsync();

        var response = await Client.GetAsync("/customer");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        body!.Name.Should().Be(created.Name);
        body.Slug.Should().Be(created.Slug);
    }

    [Fact]
    public async Task Get_WithoutCustomer_Returns404()
    {
        var response = await Client.GetAsync("/customer");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
