namespace Plans.UnitTests.Features.Plans.Api.PlanAggregate.Commands;

public class ActivateProviderConfigurationTests
{
    private readonly Mock<ActivateProviderConfiguration.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly ActivateProviderConfiguration.Service _service;
    private readonly Plan.Create _createPlan;
    private readonly Plan.AddProviderConfiguration _addProviderConfig;

    public ActivateProviderConfigurationTests()
    {
        var moneyValidator = new MoneyValidator();
        var planValidator = new PlanValidator();
        var providerConfigValidator = new PaymentProviderConfigValidator();
        var createMoney = new Money.Create(moneyValidator);
        var providerConfigCreate = new PaymentProviderConfig.Create(providerConfigValidator);
        var activateProviderConfig = new Plan.ActivateProviderConfiguration(providerConfigCreate, planValidator);
        _createPlan = new Plan.Create(createMoney, planValidator);
        _addProviderConfig = new Plan.AddProviderConfiguration(providerConfigCreate, planValidator);

        _service = new ActivateProviderConfiguration.Service(
            activateProviderConfig,
            _repository.Object,
            _unitOfWork.Object
        );
    }

    private Plan CreatePlanWithProviderConfig(
        string provider = "Stripe",
        string externalProductId = "prod_123",
        string externalPriceId = "price_123")
    {
        var plan = _createPlan.Execute(new CreatePlanCommand(
            "Plan Test",
            "Descripcion de test",
            9.99m,
            "EUR",
            BillingPeriod.Monthly
        ));

        _addProviderConfig.Execute(plan, new AddProviderConfigurationCommand(
            provider,
            externalProductId,
            externalPriceId
        ));

        return plan;
    }

    [Fact]
    public async Task HandleAsync_WithValidData_GetsFromRepository()
    {
        var plan = CreatePlanWithProviderConfig();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        await _service.HandleAsync(plan.Id, "Stripe");

        _repository.Verify(r => r.Get(plan.Id), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithValidData_ActivatesProviderConfig()
    {
        var plan = CreatePlanWithProviderConfig();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        await _service.HandleAsync(plan.Id, "Stripe");

        var config = plan.ProviderConfigurations.First(c => c.Provider == "Stripe");
        config.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithValidData_SavesChanges()
    {
        var plan = CreatePlanWithProviderConfig();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        await _service.HandleAsync(plan.Id, "Stripe");

        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithValidData_ReturnsPlanResponse()
    {
        var plan = CreatePlanWithProviderConfig();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        var response = await _service.HandleAsync(plan.Id, "Stripe");

        response.Id.Should().Be(plan.Id);
        response.Name.Should().Be("Plan Test");
        var config = response.ProviderConfigurations.First(c => c.Provider == "Stripe");
        config.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithNonExistingProvider_ThrowsKeyNotFoundException()
    {
        var plan = CreatePlanWithProviderConfig();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        var act = () => _service.HandleAsync(plan.Id, "NonExistent");

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WithNotFoundPlan_ThrowsKeyNotFoundException()
    {
        var nonExistentId = Guid.NewGuid();
        _repository.Setup(r => r.Get(nonExistentId))
            .ThrowsAsync(new KeyNotFoundException());

        var act = () => _service.HandleAsync(nonExistentId, "Stripe");

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handler_ReturnsOkWithPlanResponse()
    {
        var mockService = new Mock<ActivateProviderConfiguration.IService>();
        var id = Guid.NewGuid();
        var expectedResponse = new PlanResponse(
            id,
            "Plan Test",
            "Descripcion",
            new MoneyResponse(9.99m, new CurrencyResponse("EUR", "E", 2)),
            BillingPeriod.Monthly,
            false,
            true,
            [],
            [new ProviderConfigResponse("Stripe", "prod_123", "price_123", true)]
        );
        mockService.Setup(s => s.HandleAsync(id, "Stripe"))
            .ReturnsAsync(expectedResponse);

        var result = await ActivateProviderConfiguration.Handler(mockService.Object, id, "Stripe");

        var okResult = result.Should().BeOfType<Ok<PlanResponse>>().Subject;
        okResult.Value.Should().Be(expectedResponse);
    }
}
