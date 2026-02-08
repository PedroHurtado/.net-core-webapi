namespace Customers.IntegrationTests.Customers.Api.CustomerAggregateTests.Commands;

public class AddImageTests(WebApplicationFactory<Program> factory)
    : CustomersWebApplicationFixture(factory)
{
    [Fact]
    public async Task AddImage_WithValidData_Returns201AndPersistsData()
    {
        await Client.CreateCustomerAsync();

        var request = new AddImage.Request(
            Url: "https://cdn.test.com/images/fachada.jpg",
            AltText: "Fachada del bar",
            DisplayOrder: 0,
            IsCover: true);

        var response = await Client.PostAsJsonAsync("/customer/images", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Read-after-write
        var getResponse = await Client.GetAsync("/customer");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var persisted = await getResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        persisted!.HasImages.Should().BeTrue();
        persisted.Images.Should().ContainSingle();
        persisted.Images.First().Url.Should().Be("https://cdn.test.com/images/fachada.jpg");
        persisted.Images.First().IsCover.Should().BeTrue();
    }

    [Fact]
    public async Task AddImage_WithoutCustomer_Returns404()
    {
        var request = new AddImage.Request(
            Url: "https://cdn.test.com/images/fachada.jpg",
            AltText: null,
            DisplayOrder: 0,
            IsCover: false);

        var response = await Client.PostAsJsonAsync("/customer/images", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddImage_WithDuplicateUrl_Returns409()
    {
        await Client.CreateCustomerWithImageAsync(
            imageUrl: "https://cdn.test.com/images/fachada.jpg");

        var request = new AddImage.Request(
            Url: "https://cdn.test.com/images/fachada.jpg",
            AltText: null,
            DisplayOrder: 1,
            IsCover: false);

        var response = await Client.PostAsJsonAsync("/customer/images", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AddImage_WithInvalidData_Returns422()
    {
        await Client.CreateCustomerAsync();

        var request = new AddImage.Request(
            Url: "not-a-url",
            AltText: null,
            DisplayOrder: 0,
            IsCover: false);

        var response = await Client.PostAsJsonAsync("/customer/images", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
