namespace Customers.IntegrationTests.Helpers;

public static class CreateCustomerWithSupportedCultureHelper
{
    public static async Task<CustomerResponse> CreateCustomerWithSupportedCultureAsync(
        this HttpClient client,
        string? slug = null,
        string cultureCode = "en-GB")
    {
        await client.CreateCustomerAsync(slug);

        var request = new AddSupportedCulture.Request(Code: cultureCode);

        var response = await client.PostAsJsonAsync("/customer/supported-cultures", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var getResponse = await client.GetAsync("/customer");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await getResponse.Content.ReadFromJsonAsync<CustomerResponse>())!;
    }
}
