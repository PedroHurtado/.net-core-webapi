namespace Customers.IntegrationTests.Helpers;

public static class CreateCompleteCustomerHelper
{
    public static async Task<CustomerResponse> CreateCompleteCustomerAsync(
        this HttpClient client, string? slug = null)
    {
        var created = await client.CreateCustomerAsync(slug);

        var updateRequest = new UpdateCustomer.Request(
            Name: "Test Customer",
            Slug: created.Slug,
            Description: "A complete test customer",
            LogoUrl: "https://cdn.test.com/logo.jpg",
            EstablishmentType: "Bar",
            DefaultCulture: "es-ES",
            TimeZoneId: "Europe/Madrid",
            CuisineTypes: ["Española", "Mediterránea"],
            ServiceAmenities: [],
            DietaryOptions: []);

        var updateResponse = await client.PutAsJsonAsync("/customer", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var imageRequest = new AddImage.Request(
            Url: "https://cdn.test.com/images/fachada.jpg",
            AltText: "Fachada",
            DisplayOrder: 0,
            IsCover: true);

        var imageResponse = await client.PostAsJsonAsync("/customer/images", imageRequest);
        imageResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var getResponse = await client.GetAsync("/customer");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var customer = (await getResponse.Content.ReadFromJsonAsync<CustomerResponse>())!;
        customer.IsProfileComplete.Should().BeTrue();
        return customer;
    }
}
