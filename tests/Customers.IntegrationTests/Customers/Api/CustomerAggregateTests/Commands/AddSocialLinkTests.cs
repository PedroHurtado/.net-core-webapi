namespace Customers.IntegrationTests.Customers.Api.CustomerAggregateTests.Commands;

public class AddSocialLinkTests(WebApplicationFactory<Program> factory)
    : CustomersWebApplicationFixture(factory)
{
    [Fact]
    public async Task AddSocialLink_WithValidData_Returns201AndPersistsData()
    {
        await Client.CreateCustomerAsync();

        var request = new AddSocialLink.Request(
            Platform: "Facebook",
            Url: "https://facebook.com/elbardeljuanjo");

        var response = await Client.PostAsJsonAsync("/customer/social-links", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Read-after-write
        var getResponse = await Client.GetAsync("/customer");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var persisted = await getResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        persisted!.SocialLinks.Should().ContainSingle();
        persisted.SocialLinks.First().Platform.Should().Be("Facebook");
        persisted.SocialLinks.First().Url.Should().Be("https://facebook.com/elbardeljuanjo");
    }

    [Fact]
    public async Task AddSocialLink_WithoutCustomer_Returns404()
    {
        var request = new AddSocialLink.Request(
            Platform: "Facebook",
            Url: "https://facebook.com/test");

        var response = await Client.PostAsJsonAsync("/customer/social-links", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddSocialLink_WithDuplicatePlatform_Returns409()
    {
        await Client.CreateCustomerWithSocialLinkAsync(
            platform: "Facebook",
            url: "https://facebook.com/elbardeljuanjo");

        var request = new AddSocialLink.Request(
            Platform: "Facebook",
            Url: "https://facebook.com/otro");

        var response = await Client.PostAsJsonAsync("/customer/social-links", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AddSocialLink_WithInvalidData_Returns422()
    {
        await Client.CreateCustomerAsync();

        var request = new AddSocialLink.Request(
            Platform: "Facebook",
            Url: "not-a-url");

        var response = await Client.PostAsJsonAsync("/customer/social-links", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
