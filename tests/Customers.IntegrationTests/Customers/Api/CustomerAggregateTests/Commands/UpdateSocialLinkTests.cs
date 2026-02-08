namespace Customers.IntegrationTests.Customers.Api.CustomerAggregateTests.Commands;

public class UpdateSocialLinkTests(WebApplicationFactory<Program> factory)
    : CustomersWebApplicationFixture(factory)
{
    [Fact]
    public async Task UpdateSocialLink_WithValidData_Returns204AndPersistsChanges()
    {
        await Client.CreateCustomerWithSocialLinkAsync(
            platform: "Facebook",
            url: "https://facebook.com/elbardeljuanjo");

        var request = new UpdateSocialLink.Request(
            Url: "https://facebook.com/bardeljuanjo-nuevo");

        var response = await Client.PutAsJsonAsync("/customer/social-links/Facebook", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Read-after-write
        var getResponse = await Client.GetAsync("/customer");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var persisted = await getResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        var link = persisted!.SocialLinks.First(s => s.Platform == "Facebook");
        link.Url.Should().Be("https://facebook.com/bardeljuanjo-nuevo");
    }

    [Fact]
    public async Task UpdateSocialLink_WithoutCustomer_Returns404()
    {
        var request = new UpdateSocialLink.Request(
            Url: "https://facebook.com/test");

        var response = await Client.PutAsJsonAsync("/customer/social-links/Facebook", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateSocialLink_WithNonExistentPlatform_Returns404()
    {
        await Client.CreateCustomerAsync();

        var request = new UpdateSocialLink.Request(
            Url: "https://tripadvisor.es/test");

        var response = await Client.PutAsJsonAsync("/customer/social-links/TripAdvisor", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
