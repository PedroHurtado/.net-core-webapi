namespace Plans.IntegrationTests.Features.Plans.Api.PlanAggregate.Commands;

public class CreatePlanTests : PlanWebApplicationFixture
{
    public CreatePlanTests(WebApplicationFactory<Program> factory)
        : base(factory) { }

    [Fact]
    public async Task Create_WithValidData_Returns201()
    {
        var request = new
        {
            Name = "Plan Básico",
            Description = "Ideal para empezar",
            Amount = 9.99m,
            CurrencyCode = "EUR",
            BillingPeriod = "Monthly"
        };

        var response = await Client.PostAsJsonAsync("/plans", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var content = await response.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);
        content.Should().NotBeNull();
        content!.Id.Should().NotBeEmpty();
        content.Name.Should().Be("Plan Básico");
        content.Description.Should().Be("Ideal para empezar");
        content.Price.Amount.Should().Be(9.99m);
        content.Price.Currency.Code.Should().Be("EUR");
        content.BillingPeriod.Should().Be(BillingPeriod.Monthly);
        content.IsActive.Should().BeFalse();
        content.Features.Should().BeEmpty();
        content.ProviderConfigurations.Should().BeEmpty();
    }

    [Fact]
    public async Task Create_WithEmptyName_Returns422()
    {
        var request = new
        {
            Name = "",
            Description = "Descripción válida",
            Amount = 9.99m,
            CurrencyCode = "EUR",
            BillingPeriod = "Monthly"
        };

        var response = await Client.PostAsJsonAsync("/plans", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Create_WithEmptyDescription_Returns422()
    {
        var request = new
        {
            Name = "Plan Válido",
            Description = "",
            Amount = 9.99m,
            CurrencyCode = "EUR",
            BillingPeriod = "Monthly"
        };

        var response = await Client.PostAsJsonAsync("/plans", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Create_WithNegativeAmount_Returns422()
    {
        var request = new
        {
            Name = "Plan Válido",
            Description = "Descripción válida",
            Amount = -5m,
            CurrencyCode = "EUR",
            BillingPeriod = "Monthly"
        };

        var response = await Client.PostAsJsonAsync("/plans", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Create_WithInvalidCurrencyCode_Returns422()
    {
        var request = new
        {
            Name = "Plan Válido",
            Description = "Descripción válida",
            Amount = 9.99m,
            CurrencyCode = "XXX",
            BillingPeriod = "Monthly"
        };

        var response = await Client.PostAsJsonAsync("/plans", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
