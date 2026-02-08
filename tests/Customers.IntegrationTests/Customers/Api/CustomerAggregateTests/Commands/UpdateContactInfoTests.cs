namespace Customers.IntegrationTests.Customers.Api.CustomerAggregateTests.Commands;

public class UpdateContactInfoTests(WebApplicationFactory<Program> factory)
    : CustomersWebApplicationFixture(factory)
{
    [Fact]
    public async Task UpdateContactInfo_WithValidData_Returns204AndPersistsChanges()
    {
        await Client.CreateCustomerAsync();

        var request = new UpdateContactInfo.Request(
            Phone: "639079482",
            Email: "juanjo@bar.com",
            WebsiteUrl: "https://elbardeljuanjo.com");

        var response = await Client.PutAsJsonAsync("/customer/contact-info", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Read-after-write
        var getResponse = await Client.GetAsync("/customer");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var persisted = await getResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        persisted!.ContactInfo.Phone.Should().Be("639079482");
        persisted.ContactInfo.Email.Should().Be("juanjo@bar.com");
        persisted.ContactInfo.WebsiteUrl.Should().Be("https://elbardeljuanjo.com");
    }

    [Fact]
    public async Task UpdateContactInfo_WithoutCustomer_Returns404()
    {
        var request = new UpdateContactInfo.Request(
            Phone: "600000000",
            Email: null,
            WebsiteUrl: null);

        var response = await Client.PutAsJsonAsync("/customer/contact-info", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateContactInfo_WithInvalidData_Returns422()
    {
        await Client.CreateCustomerAsync();

        var request = new UpdateContactInfo.Request(
            Phone: "",
            Email: null,
            WebsiteUrl: null);

        var response = await Client.PutAsJsonAsync("/customer/contact-info", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
