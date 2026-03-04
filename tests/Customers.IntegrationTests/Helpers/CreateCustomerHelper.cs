namespace Customers.IntegrationTests.Helpers;

public static class CreateCustomerHelper
{
    public static async Task<CustomerResponse> CreateCustomerAsync(
        this HttpClient client, string? slug = null)
    {
        slug ??= $"test-{Guid.NewGuid():N}";

        var request = new CreateCustomer.Request(
            Id: Guid.NewGuid(),
            Name: "Test Customer",
            Slug: slug,
            Description: null,
            EstablishmentType: "Bar",
            DefaultCulture: "es-ES",
            TimeZoneId: "Europe/Madrid",
            Address: new CreateCustomer.CreateAddressRequest(
                Street: "Ctra. Murcia, 23",
                City: "La Puebla de Mula",
                PostalCode: "30193",
                Region: "Murcia",
                Country: "España",
                Latitude: 38.0389m,
                Longitude: -1.4917m),
            ContactInfo: new CreateCustomer.CreateContactInfoRequest(
                Phone: "639079481",
                Email: null,
                WebsiteUrl: null),
            BillingInfo: new CreateCustomer.CreateBillingInfoRequest(
                BusinessName: "Test SL",
                TaxId: "B12345678",
                BillingAddress: new CreateCustomer.CreateAddressRequest(
                    Street: "Ctra. Murcia, 23",
                    City: "La Puebla de Mula",
                    PostalCode: "30193",
                    Region: "Murcia",
                    Country: "España",
                    Latitude: 38.0389m,
                    Longitude: -1.4917m)));

        var response = await client.PostAsJsonAsync("/customer", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var getResponse = await client.GetAsync("/customer");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await getResponse.Content.ReadFromJsonAsync<CustomerResponse>())!;
    }
}
