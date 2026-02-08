namespace Customers.IntegrationTests.Customers.Api.CustomerAggregateTests.Commands;

public class AddSupportedCultureTests(WebApplicationFactory<Program> factory)
    : CustomersWebApplicationFixture(factory)
{
    [Fact]
    public async Task AddSupportedCulture_WithValidData_Returns201AndPersistsData()
    {
        await Client.CreateCustomerAsync();

        var request = new AddSupportedCulture.Request(Code: "en-GB");

        var response = await Client.PostAsJsonAsync("/customer/supported-cultures", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Read-after-write
        var getResponse = await Client.GetAsync("/customer");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var persisted = await getResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        persisted!.SupportedCultures.Should().ContainSingle();
        persisted.SupportedCultures.First().Code.Should().Be("en-GB");
    }

    [Fact]
    public async Task AddSupportedCulture_WithoutCustomer_Returns404()
    {
        var request = new AddSupportedCulture.Request(Code: "en-GB");

        var response = await Client.PostAsJsonAsync("/customer/supported-cultures", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddSupportedCulture_WithDuplicate_Returns409()
    {
        await Client.CreateCustomerWithSupportedCultureAsync(cultureCode: "en-GB");

        var request = new AddSupportedCulture.Request(Code: "en-GB");

        var response = await Client.PostAsJsonAsync("/customer/supported-cultures", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AddSupportedCulture_WithInvalidData_Returns422()
    {
        await Client.CreateCustomerAsync();

        var request = new AddSupportedCulture.Request(Code: "english");

        var response = await Client.PostAsJsonAsync("/customer/supported-cultures", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
