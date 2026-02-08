namespace Customers.IntegrationTests.Customers.Api.CustomerAggregateTests.Commands;

public class UpdateImageTests(WebApplicationFactory<Program> factory)
    : CustomersWebApplicationFixture(factory)
{
    [Fact]
    public async Task UpdateImage_WithValidData_Returns204AndPersistsChanges()
    {
        var customer = await Client.CreateCustomerWithImageAsync();
        var imageId = customer.Images.First().Id;

        var request = new UpdateImage.Request(
            AltText: "Fachada renovada",
            DisplayOrder: 2,
            IsCover: true);

        var response = await Client.PutAsJsonAsync($"/customer/images/{imageId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Read-after-write
        var getResponse = await Client.GetAsync("/customer");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var persisted = await getResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        var image = persisted!.Images.First(i => i.Id == imageId);
        image.AltText.Should().Be("Fachada renovada");
        image.DisplayOrder.Should().Be(2);
    }

    [Fact]
    public async Task UpdateImage_WithoutCustomer_Returns404()
    {
        var request = new UpdateImage.Request(
            AltText: "Test",
            DisplayOrder: 0,
            IsCover: false);

        var response = await Client.PutAsJsonAsync($"/customer/images/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateImage_WithInvalidImageId_Returns404()
    {
        await Client.CreateCustomerAsync();

        var request = new UpdateImage.Request(
            AltText: "Test",
            DisplayOrder: 0,
            IsCover: false);

        var response = await Client.PutAsJsonAsync($"/customer/images/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
