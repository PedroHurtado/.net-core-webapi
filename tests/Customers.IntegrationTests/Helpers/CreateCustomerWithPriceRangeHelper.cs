namespace Customers.IntegrationTests.Helpers;

public static class CreateCustomerWithPriceRangeHelper
{
    public static async Task<CustomerResponse> CreateCustomerWithPriceRangeAsync(
        this HttpClient client,
        string? slug = null,
        decimal minPrice = 10m,
        decimal maxPrice = 30m)
    {
        await client.CreateCustomerAsync(slug);

        var request = new SetPriceRange.Request(
            MinPrice: minPrice,
            MaxPrice: maxPrice);

        var response = await client.PutAsJsonAsync("/customer/price-range", request);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync("/customer");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await getResponse.Content.ReadFromJsonAsync<CustomerResponse>())!;
    }
}
