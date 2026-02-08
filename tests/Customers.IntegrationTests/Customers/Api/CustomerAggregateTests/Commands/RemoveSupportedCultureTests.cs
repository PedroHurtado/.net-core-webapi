namespace Customers.IntegrationTests.Customers.Api.CustomerAggregateTests.Commands;

public class RemoveSupportedCultureTests(WebApplicationFactory<Program> factory)
    : CustomersWebApplicationFixture(factory)
{
    [Fact]
    public async Task RemoveSupportedCulture_WithExisting_Returns204AndPersistsChanges()
    {
        await Client.CreateCustomerWithSupportedCultureAsync(cultureCode: "en-GB");

        var response = await Client.DeleteAsync("/customer/supported-cultures/en-GB");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Read-after-write
        var getResponse = await Client.GetAsync("/customer");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var persisted = await getResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        persisted!.SupportedCultures.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveSupportedCulture_WithoutCustomer_Returns404()
    {
        var response = await Client.DeleteAsync("/customer/supported-cultures/en-GB");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
