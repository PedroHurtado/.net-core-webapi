namespace Customers.IntegrationTests.Customers.Api.CustomerAggregateTests.Commands;

public class RemoveSocialLinkTests(WebApplicationFactory<Program> factory)
    : CustomersWebApplicationFixture(factory)
{
    [Fact]
    public async Task RemoveSocialLink_WithExisting_Returns204AndPersistsChanges()
    {
        await Client.CreateCustomerWithSocialLinkAsync(
            platform: "Facebook",
            url: "https://facebook.com/elbardeljuanjo");

        var response = await Client.DeleteAsync("/customer/social-links/Facebook");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Read-after-write
        var getResponse = await Client.GetAsync("/customer");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var persisted = await getResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        persisted!.SocialLinks.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveSocialLink_WithoutCustomer_Returns404()
    {
        var response = await Client.DeleteAsync("/customer/social-links/Facebook");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
