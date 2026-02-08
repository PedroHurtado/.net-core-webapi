namespace Customers.IntegrationTests.Customers.Api.CustomerAggregateTests.Commands;

public class RemoveImageTests(WebApplicationFactory<Program> factory)
    : CustomersWebApplicationFixture(factory)
{
    [Fact]
    public async Task RemoveImage_WithExisting_Returns204AndPersistsChanges()
    {
        var customer = await Client.CreateCustomerWithImageAsync();
        var imageId = customer.Images.First().Id;

        var response = await Client.DeleteAsync($"/customer/images/{imageId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Read-after-write
        var getResponse = await Client.GetAsync("/customer");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var persisted = await getResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        persisted!.HasImages.Should().BeFalse();
        persisted.Images.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveImage_WithoutCustomer_Returns404()
    {
        var response = await Client.DeleteAsync($"/customer/images/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
