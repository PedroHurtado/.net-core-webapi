namespace Customers.IntegrationTests.Helpers;

public static class CreateActiveCustomerHelper
{
    public static async Task<CustomerResponse> CreateActiveCustomerAsync(
        this HttpClient client, string? slug = null)
    {
        await client.CreateCompleteCustomerAsync(slug);

        var response = await client.PostAsync("/customer/activate", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await client.GetAsync("/customer");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await getResponse.Content.ReadFromJsonAsync<CustomerResponse>())!;
    }
}
