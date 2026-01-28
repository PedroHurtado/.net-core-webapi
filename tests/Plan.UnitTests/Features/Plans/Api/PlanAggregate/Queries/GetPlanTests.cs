namespace Plans.UnitTests.Features.Plans.Api.PlanAggregate.Queries;

public class GetPlanTests
{
    private readonly Mock<GetPlan.IRepository> _repositoryMock = new();
    private readonly GetPlan.Service _service;
    private readonly Plan.Create _createPlan;

    public GetPlanTests()
    {
        var moneyValidator = new MoneyValidator();
        var planValidator = new PlanValidator();
        var createMoney = new Money.Create(moneyValidator);
        _createPlan = new Plan.Create(createMoney, planValidator);

        _service = new GetPlan.Service(_repositoryMock.Object);
    }

    private Plan CreatePlan(
        string name = "Plan Básico",
        string description = "Ideal para empezar",
        decimal amount = 9.99m,
        string currencyCode = "EUR",
        BillingPeriod billingPeriod = BillingPeriod.Monthly)
    {
        return _createPlan.Execute(new CreatePlanCommand(
            name,
            description,
            amount,
            currencyCode,
            billingPeriod
        ));
    }

    #region Service Tests

    [Fact]
    public async Task HandleAsync_WithExistingId_ReturnsResponse()
    {
        var plan = CreatePlan();
        _repositoryMock.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        var response = await _service.HandleAsync(plan.Id);

        response.Should().NotBeNull();
        response.Id.Should().Be(plan.Id);
        response.Name.Should().Be("Plan Básico");
    }

    [Fact]
    public async Task HandleAsync_WithAllFields_ReturnsCompleteResponse()
    {
        var plan = CreatePlan(
            name: "Plan Premium",
            description: "Para profesionales",
            amount: 29.99m,
            currencyCode: "EUR",
            billingPeriod: BillingPeriod.Yearly
        );
        _repositoryMock.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        var response = await _service.HandleAsync(plan.Id);

        response.Id.Should().Be(plan.Id);
        response.Name.Should().Be("Plan Premium");
        response.Description.Should().Be("Para profesionales");
        response.Price.Amount.Should().Be(29.99m);
        response.Price.Currency.Code.Should().Be("EUR");
        response.BillingPeriod.Should().Be(BillingPeriod.Yearly);
        response.IsActive.Should().BeFalse();
        response.Features.Should().BeEmpty();
        response.ProviderConfigurations.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var plan = CreatePlan();
        _repositoryMock.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        await _service.HandleAsync(plan.Id);

        _repositoryMock.Verify(r => r.Get(plan.Id), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenPlanNotFound_ThrowsException()
    {
        var nonExistentId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(nonExistentId))
            .ThrowsAsync(new KeyNotFoundException("Plan not found"));

        var act = () => _service.HandleAsync(nonExistentId);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidId_ReturnsOkResult()
    {
        var serviceMock = new Mock<GetPlan.IService>();
        var expectedId = Guid.NewGuid();
        var expectedResponse = new PlanResponse(
            expectedId,
            "Plan Test",
            "Descripción",
            new MoneyResponse(9.99m, new CurrencyResponse("EUR", "€", 2)),
            BillingPeriod.Monthly,
            false,
            false,
            [],
            []
        );

        serviceMock.Setup(s => s.HandleAsync(expectedId)).ReturnsAsync(expectedResponse);

        var result = await GetPlan.Handler(serviceMock.Object, expectedId);

        result.Should().BeOfType<Ok<PlanResponse>>();
        var okResult = (Ok<PlanResponse>)result;
        okResult.Value.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task Handler_CallsServiceWithId()
    {
        var serviceMock = new Mock<GetPlan.IService>();
        var expectedId = Guid.NewGuid();
        var expectedResponse = new PlanResponse(
            expectedId,
            "Plan Test",
            "Descripción",
            new MoneyResponse(9.99m, new CurrencyResponse("EUR", "€", 2)),
            BillingPeriod.Monthly,
            false,
            false,
            [],
            []
        );

        serviceMock.Setup(s => s.HandleAsync(It.IsAny<Guid>())).ReturnsAsync(expectedResponse);

        await GetPlan.Handler(serviceMock.Object, expectedId);

        serviceMock.Verify(s => s.HandleAsync(expectedId), Times.Once);
    }

    #endregion
}
