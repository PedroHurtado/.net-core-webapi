namespace Plans.IntegrationTests.Features.Plans.Api.PlanAggregate.Commands;

public class UpdatePlanTests : PlanWebApplicationFixture
{
    public UpdatePlanTests(WebApplicationFactory<Program> factory)
        : base(factory) { }

    [Fact]
    public async Task Update_WithValidData_Returns204()
    {
        var created = await CreatePlanAsync();
        var request = new
        {
            Name = "Plan Actualizado",
            Description = "Nueva descripción",
            Amount = 12.99m,
            CurrencyCode = "EUR",
            BillingPeriod = "Monthly"
        };

        var response = await Client.PutAsJsonAsync($"/plans/{created.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_WithValidData_PersistsChanges()
    {
        var created = await CreatePlanAsync();
        var request = new
        {
            Name = "Plan Actualizado",
            Description = "Nueva descripción",
            Amount = 12.99m,
            CurrencyCode = "USD",
            BillingPeriod = "Yearly"
        };

        await Client.PutAsJsonAsync($"/plans/{created.Id}", request);

        var getResponse = await Client.GetAsync($"/plans/{created.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Plan Actualizado");
        result.Description.Should().Be("Nueva descripción");
        result.Price.Amount.Should().Be(12.99m);
        result.Price.Currency.Code.Should().Be("USD");
        result.BillingPeriod.Should().Be(BillingPeriod.Yearly);
    }

    [Fact]
    public async Task Update_WithNonExistingId_Returns404()
    {
        var nonExistentId = Guid.NewGuid();
        var request = new
        {
            Name = "Plan Actualizado",
            Description = "Nueva descripción",
            Amount = 12.99m,
            CurrencyCode = "EUR",
            BillingPeriod = "Monthly"
        };

        var response = await Client.PutAsJsonAsync($"/plans/{nonExistentId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WithEmptyName_Returns422()
    {
        var created = await CreatePlanAsync();
        var request = new
        {
            Name = "",
            Description = "Descripción válida",
            Amount = 9.99m,
            CurrencyCode = "EUR",
            BillingPeriod = "Monthly"
        };

        var response = await Client.PutAsJsonAsync($"/plans/{created.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}