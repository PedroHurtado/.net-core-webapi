namespace Customers.IntegrationTests.Helpers;

public static class CreateCustomerWithSocialLinkHelper
{
    public static async Task<CustomerResponse> CreateCustomerWithSocialLinkAsync(
        this HttpClient client,
        string? slug = null,
        string platform = "Facebook",
        string url = "https://facebook.com/test")
    {
        await client.CreateCustomerAsync(slug);

        var request = new AddSocialLink.Request(
            Platform: platform,
            Url: url);

        var response = await client.PostAsJsonAsync("/customer/social-links", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var getResponse = await client.GetAsync("/customer");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await getResponse.Content.ReadFromJsonAsync<CustomerResponse>())!;
    }
}
