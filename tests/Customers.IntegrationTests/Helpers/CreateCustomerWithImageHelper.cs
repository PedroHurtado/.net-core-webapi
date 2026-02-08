namespace Customers.IntegrationTests.Helpers;

public static class CreateCustomerWithImageHelper
{
    public static async Task<CustomerResponse> CreateCustomerWithImageAsync(
        this HttpClient client,
        string? slug = null,
        string imageUrl = "https://cdn.test.com/images/fachada.jpg",
        bool isCover = true)
    {
        await client.CreateCustomerAsync(slug);

        var request = new AddImage.Request(
            Url: imageUrl,
            AltText: "Test image",
            DisplayOrder: 0,
            IsCover: isCover);

        var response = await client.PostAsJsonAsync("/customer/images", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var getResponse = await client.GetAsync("/customer");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await getResponse.Content.ReadFromJsonAsync<CustomerResponse>())!;
    }
}
