namespace Plans.UnitTests.Features.Plans.Api.PlanAggregate.Commands;

public class UpdateProviderConfigurationTests
{
    private readonly Mock<UpdateProviderConfiguration.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly UpdateProviderConfiguration.Service _service;
    private readonly Plan.Create _createPlan;
    private readonly Plan.AddProviderConfiguration _addProviderConfig;

    public UpdateProviderConfigurationTests()
    {
        var moneyValidator = new MoneyValidator();
        var planValidator = new PlanValidator();
        var providerConfigValidator = new PaymentProviderConfigValidator();
        var createMoney = new Money.Create(moneyValidator);
        var providerConfigCreate = new PaymentProviderConfig.Create(providerConfigValidator);
        var updateProviderConfig = new Plan.UpdateProviderConfiguration(providerConfigCreate, planValidator);
        _createPlan = new Plan.Create(createMoney, planValidator);
        _addProviderConfig = new Plan.AddProviderConfiguration(providerConfigCreate, planValidator);

        _service = new UpdateProviderConfiguration.Service(
            updateProviderConfig,
            _repository.Object,
            _unitOfWork.Object
        );
    }

    private Plan CreatePlanWithProviderConfig(
        string provider = "Stripe",
        string externalProductId = "old_prod",
        string externalPriceId = "old_price")
    {
        var plan = _createPlan.Execute(new CreatePlanCommand(
            "Plan Test",
            "Descripción de test",
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

        var request = new UpdateProviderConfiguration.Request(
            ExternalProductId: "new_prod",
            ExternalPriceId: "new_price"
        );

        await _service.HandleAsync(plan.Id, "Stripe", request);

        _repository.Verify(r => r.Get(plan.Id), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithValidData_UpdatesProviderConfig()
    {
        var plan = CreatePlanWithProviderConfig();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        var request = new UpdateProviderConfiguration.Request(
            ExternalProductId: "new_prod",
            ExternalPriceId: "new_price"
        );

        await _service.HandleAsync(plan.Id, "Stripe", request);

        var config = plan.ProviderConfigurations.First(c => c.Provider == "Stripe");
        config.ExternalProductId.Should().Be("new_prod");
        config.ExternalPriceId.Should().Be("new_price");
    }

    [Fact]
    public async Task HandleAsync_WithValidData_SavesChanges()
    {
        var plan = CreatePlanWithProviderConfig();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        var request = new UpdateProviderConfiguration.Request(
            ExternalProductId: "new_prod",
            ExternalPriceId: "new_price"
        );

        await _service.HandleAsync(plan.Id, "Stripe", request);

        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistingProvider_ThrowsKeyNotFoundException()
    {
        var plan = CreatePlanWithProviderConfig();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        var request = new UpdateProviderConfiguration.Request(
            ExternalProductId: "new_prod",
            ExternalPriceId: "new_price"
        );

        var act = () => _service.HandleAsync(plan.Id, "NonExistent", request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WithInvalidData_ThrowsValidationException()
    {
        var plan = CreatePlanWithProviderConfig();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        var request = new UpdateProviderConfiguration.Request(
            ExternalProductId: "",
            ExternalPriceId: ""
        );

        var act = () => _service.HandleAsync(plan.Id, "Stripe", request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithNotFoundPlan_ThrowsKeyNotFoundException()
    {
        var nonExistentId = Guid.NewGuid();
        _repository.Setup(r => r.Get(nonExistentId))
            .ThrowsAsync(new KeyNotFoundException());

        var request = new UpdateProviderConfiguration.Request(
            ExternalProductId: "new_prod",
            ExternalPriceId: "new_price"
        );

        var act = () => _service.HandleAsync(nonExistentId, "Stripe", request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handler_ReturnsNoContent()
    {
        var mockService = new Mock<UpdateProviderConfiguration.IService>();
        var id = Guid.NewGuid();

        var request = new UpdateProviderConfiguration.Request(
            ExternalProductId: "new_prod",
            ExternalPriceId: "new_price"
        );

        var result = await UpdateProviderConfiguration.Handler(mockService.Object, id, "Stripe", request);

        result.Should().BeOfType<NoContent>();
    }
}